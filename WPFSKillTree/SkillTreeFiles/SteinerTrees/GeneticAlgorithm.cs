using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public struct GeneticAlgorithmParameters
    {
        public readonly int MaxGeneration;

        public readonly int PopulationSize;

        public readonly int DnaLength;

        public readonly double Temperature;

        public readonly double AnnealingFactor;

        public GeneticAlgorithmParameters(int maxGeneration, int populationSize, int dnaLength,
            double temperature, double annealingFactor)
        {
            MaxGeneration = maxGeneration;

            PopulationSize = populationSize;

            DnaLength = dnaLength;

            Temperature = temperature;

            AnnealingFactor = annealingFactor;
        }
    }

    /// <summary>
    ///  Implements a genetic algorithm.
    ///  Please see the code documentation inside the class for more information.
    /// </summary>
    /// <remarks>
    /// While this class can't/shouldn't hold model knowledge and is only connected
    /// to the problem via the SolutionFitnessFunction, it is encouraged to adapt
    /// this code to the problem at hand. In particular, variable length DNA is not
    /// implemented and the mutation/crossover probabilities involved are best
    /// suited for certain shapes of fitness functions.
    /// 
    /// Also see the NFL-theorem.
    /// </remarks>
    public class GeneticAlgorithm
    {
        ///////////////////////////////////////////////////////////////////////////
        /// This genetic algorithm involves the standard two operations (mutation
        /// and crossover) performed on the DNA of the individuals comprised by
        /// the population, with a few twists.
        /// 
        /// Currently, a few things have been altered from standard procedures:
        ///  1. Half the population (the lower fitness half) is culled from the
        ///     pool every generation. New members are generated from the DNA
        ///     crossover (this part is still fairly standard, the choice of "half"
        ///     the population is arbitrary though).
        ///     Any surviving member is undergoes 3.
        ///  2. An individual must have survived a previous round of culling to
        ///     procreate via crossover. This seems to significantly improve the
        ///     quality of the crossover'd solutions without affecting genetic
        ///     diversity.
        ///  3. Mutations can be rejected:
        ///     The DNA is mutated (a single bit is flipped) and always accepted
        ///     if it results in a higher fitness value, otherwise the mutation is
        ///     discarded if a random roll (hardened by a higher fitness difference)
        ///     fails.
        ///     
        /// The first alteration ensures that the evolutionary pressure is kept on,
        /// in order to ensure a good pace of search progress.
        /// 
        /// The second one, as mentioned above, significantly reduces the amount of
        /// low fitness (therefore immediately discarded) DNA introduced from cross-
        /// overs.
        /// 
        /// The chance to reject inferior mutations is borrowed from the concept
        /// of simulated annealing. Generally a lot of potential steiner nodes
        /// are just far off and will never contribute to a better solution, so
        /// doing this "sanity" check helps keeping the dna quality in the pool high.
        /// 
        /// 
        /// For the actual crossover, two DNA "parents" are chosen at random (each
        /// via a random sample from "mature" individuals), with each individual's
        /// chance being proportional to its fitness. More precisely, fitness values
        /// are normalized to the 0-1 range (based on the so far observed max and
        /// min fitness) and used as the respective individual's weight in the
        /// random sampling. This ensures that the crossover in general is quite
        /// agnostic to the absolute values of the fitness function and therefore
        /// works similarly for all inputs.
        /// Also see WeightedSampling.
        /// 
        /// New individuals are generated parallelized. Since the fitness function
        /// is often the bottleneck this greatly increases performance.
        /// However it means that the fitness function must be thread-safe.


        /// <summary>
        ///  The fitness function to be used for evaluating individuals.
        /// </summary>
        /// <param name="DNA">The bitstring encoding the solution to be
        /// evaluated.</param>
        /// <returns>The fitness of the DNA, a score for how good the
        /// corresponding solution is.</returns>
        public delegate double SolutionFitnessFunction(BitArray DNA);

        readonly SolutionFitnessFunction solutionFitness;

        /// Asking for delegates to convert the bitstrings to the actual objects in
        /// here (and making this class generic) would be pretty silly in my eyes.

        private int dnaLength;

        private Individual[] population;

        public int PopulationSize { get; private set; }

        private double temperature;

        private double annealingFactor;

        public int GenerationCount { get; private set; }

        private BitArray _initialSolution;

        private Individual bestSolution;

        /// <summary>
        ///  Retrieves the DNA with the highest encountered fitness value thus far.
        /// </summary>
        /// <returns>The DNA holding the current fitness record.</returns>
        public BitArray GetBestDNA()
        {
            return new BitArray(bestSolution.DNA);
        }

        private readonly Random random;

        /// <summary>
        ///  An individual, comprised of a DNA and a fitness value, for use in
        ///  the genetic algorithm.
        /// </summary>
        class Individual
        {
            // I'll refrain from making this immutable...
            public BitArray DNA;

            // Enforcing this to be set in the constructor (and never changed).
            public double Fitness { get; private set; }

            // The amount of generations this individual has lived.
            public int Age;

            public Individual(BitArray DNA, double fitness)
            {
                this.DNA = DNA;
                Fitness = fitness;
                this.Age = 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the genetic algorithm optimizer.
        /// </summary>
        /// <param name="solutionFitness">A delegate to the fitness function.
        /// Because of parallelization the fitness function must be thread safe</param>
        /// <param name="random">An optional Random instance to allow for seeding.
        /// If none is provided, a newly created one is used.</param>
        public GeneticAlgorithm(SolutionFitnessFunction solutionFitness, Random random = null)
        {
            // Save the fitness function
            this.solutionFitness = solutionFitness;

            this.random = random ?? new Random();
        }

        /// <summary>
        /// Initializes a new optimization run.
        /// </summary>
        /// <param name="parameters">The parameters to initialize the algorithm with</param>
        /// <param name="initialSolution">The solution to initialize the population with</param>
        public void InitializeEvolution(GeneticAlgorithmParameters parameters, BitArray initialSolution = null)
        {
            PopulationSize = parameters.PopulationSize;
            dnaLength = parameters.DnaLength;
            temperature = parameters.Temperature;
            annealingFactor = parameters.AnnealingFactor;

            bestSolution = new Individual(null, 0);

            _initialSolution = initialSolution ?? new BitArray(dnaLength);
            population = createPopulation();
            GenerationCount = 0;
            updateBestSolution();
        }

        /// <summary>
        ///  Generates populationSize random individuals.
        /// </summary>
        /// <returns>The random individuals.</returns>
        private Individual[] createPopulation()
        {
            if (PopulationSize == 0)
            {
                return new Individual[0];
            }

            Individual[] newPopulation = new Individual[PopulationSize];
            // The initial solution is included in the initial population.
            newPopulation[0] = spawnIndividual(_initialSolution);
            //for (int i = 1; i < populationSize; i++)
            Parallel.For(1, PopulationSize, i =>
            {
                newPopulation[i] = spawnIndividual(randomBitarray());
                // Without this, nothing would be allowed to breed in the first step.
                newPopulation[i].Age++;
            });
            return newPopulation;
        }

        /// <summary>
        ///  Progresses the optimization by evolving the current population.
        /// </summary>
        /// <returns>The number of generations thus far.</returns>
        public int NewGeneration()
        {
            if (population == null)
                throw new InvalidOperationException("Cannot generate a next" +
                    " generation without prior call to InitializeEvolution!");

            Individual[] newPopulation = new Individual[PopulationSize];
            int newPopIndex = 0;
            GenerationCount++;

            WeightedSampler<Individual> sampler = new WeightedSampler<Individual>();

#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double averageHealth = 0;
            double averageBitsSet = 0;
            double averageAge = 0;
            int acceptedTotal = 0;
            int acceptedWorse = 0;
#endif

            // Sort the population by fitness.
            population = population.OrderBy(ind => ind.Fitness).ToArray();

            int index = 0;
            foreach (Individual individual in population)
            {
                index++;

#if DEBUG
                averageHealth += 1500 - individual.Fitness;
                averageBitsSet += SetBits(individual.DNA);
#endif

                // Survival of the fittest (population was ordered by fitness above)
                if (index < 0.5 * PopulationSize)
                {
                    continue;
                }

                /// This seems to have a good effect on convergence speed.
                /// By only allowing solutions that survived a round of culling
                /// to procreate, the solution quality is kept high.
                if (individual.Age >= 1)
                    sampler.AddEntry(individual, individual.Fitness);
#if DEBUG
                averageAge += individual.Age;
#endif

                individual.Age++;

                newPopulation[newPopIndex] = individual;
                newPopIndex++;
            }

            //for (int i = 0; i < newPopIndex; i++)
            Parallel.For(0, newPopIndex, i =>
            {
                Individual temp = newPopulation[i];
                Individual mutation = spawnIndividual(mutateDNA(temp.DNA));

                // Lowering the age here would lead to faster convergence but would
                // make the population go extinct several times.
                mutation.Age = temp.Age;
                // Mutations have a chance to be rejected based on the fitness loss
                // relative to the non-mutated individual. See explanation above.
                if (acceptNewState(temp, mutation))
                {
#if DEBUG
                    // If you want to measure these for debugging purposes, remove the
                    // parallelization of this loop.
                    //acceptedTotal++;
                    //if (mutation.Fitness < temp.Fitness)
                    //    acceptedWorse++;
#endif
                    temp = mutation;
                }
                newPopulation[i] = temp;
            });

#if DEBUG
            stopwatch.Stop();
            //Debug.Write("Evaluation time for " + generationCount + " : ");
            //Debug.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
            //Debug.WriteLine("Average health: " + averageHealth / populationSize);
            //Debug.WriteLine("Average bits set: " + averageBitsSet / populationSize);
            //Debug.WriteLine("Average age: " + averageAge / populationSize);
            //Debug.WriteLine("Accepted new states (all/worse): " + acceptedTotal + "/" + acceptedWorse);
            //Debug.WriteLine("Sampler entries: " + sampler.EntryCount);
            
            stopwatch.Restart();
#endif

            if (!sampler.CanSample)
            {
                // This is actually a pretty serious problem.
                population = createPopulation();
                Debug.WriteLine("Entire population was infertile (Generation " +
                                   GenerationCount + ").");
                //Debug.Fail("Population went extinct, not good...");
                return GenerationCount;
            }

            // Replace purged individuals
            //for (int i = newPopIndex; i < populationSize; i++)
            Parallel.For(newPopIndex, PopulationSize, i =>
            {
                BitArray parent1 = sampler.RandomSample().DNA;
                BitArray parent2 = sampler.RandomSample().DNA;

                BitArray newDNA = combineIndividualsDNA(parent1, parent2);

                newPopulation[i] = spawnIndividual(newDNA);
            });

            population = newPopulation;

            // Doing this at the end so the last generation has a use.
            updateBestSolution();

            temperature *= annealingFactor;

#if DEBUG
            stopwatch.Stop();
            //Debug.WriteLine("Best value so far: " + (1500 - bestSolution.Fitness));
            //Debug.WriteLine("------------------");
            //Debug.Out.Flush();
#endif

            return GenerationCount;
        }

        /// <summary>
        ///  Checks the current population for a better solution than bestSolution
        ///  and changes bestSolution if there is a better individual.
        ///  Also checks each fitness for being negative and throws an Exception
        ///  in that case.
        /// </summary>
        private void updateBestSolution()
        {
            foreach (Individual individual in population)
            {
                if (individual.Fitness < 0)
                    throw new ArgumentOutOfRangeException("solutionFitness function",
                        "Negative fitness values are not allowed! Use 0 fitness " +
                        "for solutions that should not reproduce.");

                if (individual.Fitness > bestSolution.Fitness)
                    bestSolution = new Individual(individual.DNA, individual.Fitness);
            }
        }

        /// <summary>
        ///  Factory method for generating a new individual from a DNA. Passing the
        ///  fitness function to every individual is weird, so that gets done here.
        /// </summary>
        /// <param name="dna">The DNA of the new individual.</param>
        /// <returns>The new individual.</returns>
        private Individual spawnIndividual(BitArray dna)
        {
            Individual individual = new Individual(dna, solutionFitness(dna));
            return individual;
        }

        #region DNA mutation
        /// <summary>
        ///  Flips a random bit in the passed DNA bitstring and returns the result.
        /// </summary>
        /// <param name="dna">The DNA to be mutated.</param>
        /// <returns>The mutated DNA.</returns>
        private BitArray mutateDNA(BitArray dna)
        {
            BitArray newDNA = new BitArray(dna);
            int index = random.Next(newDNA.Length);
            newDNA[index] = !newDNA[index];
            return newDNA;
        }

        /// <summary>
        ///  Creates a new DNA bitstring from two input ones. A (random) sequence
        ///  of one input DNA is "filled up" with the data of the other one.
        /// </summary>
        /// <param name="dna1">The first input DNA.</param>
        /// <param name="dna2">The second input DNA:</param>
        /// <returns>The combined DNA.</returns>
        private BitArray combineIndividualsDNA(BitArray dna1, BitArray dna2)
        {
            int length = dna1.Length;
            if (dna2.Length != length)
                throw new NotImplementedException("Breeding of individuals with" +
                            " differing DNA lengths is not yet implemented!");

            int crossoverStart = random.Next(length);
            int crossoverEnd   = random.Next(length);

            // This prevents the crossover being biased towards exchanging
            // the middle parts of the DNA and basically never affecting the
            // start or end of it.
            if (crossoverStart > crossoverEnd)
                return crossoverDNA(dna2, dna1, crossoverEnd, crossoverStart);
            else
                return crossoverDNA(dna1, dna2, crossoverStart, crossoverEnd);
        }

        /// <summary>
        ///  Replaces the bits in DNA1 from start to end with those from DNA2 and
        ///  returns the result.
        /// </summary>
        /// <param name="DNA1">The "base" DNA.</param>
        /// <param name="DNA2">The "overwriting" DNA.</param>
        /// <param name="start">The index of the start of the DNA exchange.</param>
        /// <param name="end">The index of the end of the DNA exchange.</param>
        /// <returns></returns>
        private BitArray crossoverDNA(BitArray DNA1, BitArray DNA2, int start, int end)
        {
            BitArray cross = new BitArray(DNA1);
            for (int i = start; i <= end; i++)
                cross[i] = DNA2[i];
            return cross;
        }

        /// <summary>
        ///  Generates a random BitArray. Currently this means that exactly one of
        ///  the bits is inverted from the initial solution.
        /// </summary>
        /// <returns>The random BitArray.</returns>
        private BitArray randomBitarray()
        {
            BitArray bitArray = new BitArray(_initialSolution);
            int i0 = random.Next(dnaLength);
            bitArray[i0] = !bitArray[i0];
            return bitArray;
        }

        /// <summary>
        ///  Returns the amount of bits that are set in the dna BitArray.
        /// </summary>
        /// <param name="dna">The BitArray whose set bits shall be counted.</param>
        /// <returns>The amount of bits set in dna.</returns>
        public static int SetBits(BitArray dna)
        {
            int sum = 0;
            for (int i = 0; i < dna.Length; i++)
                sum += (dna[i] ? 1 : 0);
            return sum;
        }
        #endregion

        /// <summary>
        ///  Takes a non-mutated individual and a mutated form of it and and decides
        ///  wether it should be replaced by the mutated individual in the population.
        ///  The mutated one always gets accepted if its fitness value is greater or
        ///  equal. If it is inferior it has a chance of e^(difference / temperature) to be
        ///  accepted.
        /// </summary>
        /// <param name="oldState">Non-mutated individual</param>
        /// <param name="newState">Mutated individual</param>
        /// <returns>True if the non-mutated individual should be replaced by
        /// the mutated individual in the population.</returns>
        private bool acceptNewState(Individual oldState, Individual newState)
        {
            double df = newState.Fitness - oldState.Fitness;
            if (df >= 0) return true;
            double acceptanceProbability = Math.Exp(df / temperature);
            if (random.NextDouble() < acceptanceProbability) return true;
            return false;
        }
    }
}
