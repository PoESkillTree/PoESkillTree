using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    ///  Implements a hybrid genetic algorithm with simulated annealing.
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
    class GeneticAnnealingAlgorithm
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
        ///  3. Mutations happen in the style of simulated annealing:
        ///     The DNA is mutated (a single bit is flipped) and always accepted
        ///     if it results in a higher fitness value, otherwise the mutation is
        ///     discarded if a random roll (hardened by a higher fitness difference
        ///     and a lower temperature, which decreases progressively) fails.
        ///     
        /// The first alteration ensures that the evolutionary pressure is kept on,
        /// in order to ensure a good pace of search progress.
        /// 
        /// The second one, as mentioned above, significantly reduces the amount of
        /// low fitness (therefore immediately discarded) DNA introduced from cross-
        /// overs. At the moment, a mutation will postpone this "maturity" by one
        /// generation, the effect of this (and alternatives) could be investigated.
        /// 
        /// Introducing simulated annealing proved to be a great way of exploring
        /// the search space, since the fitness function (for the steiner tree
        /// problem and the given skill tree) is nicely behaved. I don't have a pre-
        /// cise way of saying this, but the way adding or removing steiner nodes
        /// changes the fitness value is suited for simulated annealing in my eyes.
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


        /// <summary>
        ///  The fitness function to be used for evaluating individuals.
        /// </summary>
        /// <param name="DNA">The bitstring encoding the solution to be
        /// evaluated.</param>
        /// <returns>The fitness of the DNA, a score for how good the
        /// corresponding solution is.</returns>
        public delegate double SolutionFitnessFunction(BitArray DNA);
        SolutionFitnessFunction solutionFitness;

        /// Asking for delegates to convert the bitstrings to the actual objects in
        /// here (and making this class generic) would be pretty silly in my eyes.

        private int dnaLength;

        private Individual[] population;

        private int populationSize;
        public int PopulationSize
        { get { return populationSize; } }

        private int generationCount;
        public int GenerationCount { get { return generationCount; } }

        private Individual bestSolution;

        /// <summary>
        ///  Retrieves the DNA with the highest encountered fitness value thus far.
        /// </summary>
        /// <returns>The DNA holding the current fitness record.</returns>
        public BitArray GetBestDNA()
        {
            return new BitArray(bestSolution.DNA);
        }

        // Normalizing is not used in a any useful way. (the WeightedSampler doesn't care)
        //private double maxFitness;
        //private double minFitness;

        private Random random;

        // Used by the simulated annealing.
        private double temperature;
        private double annealingFactor;

        /// <summary>
        ///  An individual, comprised of a DNA and a fitness value, for use in
        ///  the genetic algorithm.
        /// </summary>
        class Individual
        {
            // I'll refrain from making this immutable...
            public BitArray DNA;

            // Enforcing this to be set in the constructor (and never changed).
            private double _fitness;
            public double Fitness
            { get { return _fitness; } }

            // The relative health (between 0 and 1) of this individual with
            // respect to the overall population.
            //public double normalizedFitness;

            // The amount of generations this individual has lived.
            public int Age;

            public Individual(BitArray DNA, double fitness)
            {
                this.DNA = DNA;
                _fitness = fitness;
                this.Age = 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the genetic algorithm optimizer.
        /// </summary>
        /// <param name="solutionFitness">A delegate to the fitness function.</param>
        /// <param name="random">An optional Random instance to allow for seeding.
        /// If none is provided, a newly created one is used.</param>
        public GeneticAnnealingAlgorithm(SolutionFitnessFunction solutionFitness, Random random = null)
        {
            // Save the fitness function
            this.solutionFitness = solutionFitness;

            if (random == null) this.random = new Random();
            else this.random = random;
        }

        /// <summary>
        /// Initializes a new optimization run.
        /// </summary>
        /// <param name="populationSize">The amount of individuals to be kept in
        /// the genetic pool.</param>
        /// <param name="maxGeneration">An estimate for the amount of generations
        /// to be simulated (needed for annealing schedule).</param>
        /// <param name="dnaLength">The (fixed) length of the DNA bitstrings used
        /// to encode solutions.</param>
        /// <param name="estimatedMaxFitness">An optional estimate of the maximum
        /// achievable fitness value of the fitness function. Better estimates may
        /// improve the convergence behavior by .</param>
        /// <param name="estimatedMinFitness">An optional estimate of the minimum
        /// achievable fitness value of the fitness function.</param>
        public void InitializeEvolution(int populationSize, int maxGeneration, int dnaLength, double estimatedMaxFitness = 0, double estimatedMinFitness = double.MaxValue)
            //BitArray initialSolution = null)
        {
            this.populationSize = populationSize;

            /// These assignments (based on the defaults) look paradoxical here,
            /// but they'll be adjusted by evaluating individuals before ever being
            /// used.
            //this.maxFitness = estimatedMaxFitness;
            //this.minFitness = estimatedMinFitness;

            this.dnaLength = dnaLength;

            bestSolution = new Individual(null, 0);

            population = createPopulation();
            generationCount = 0;
            updateBestSolution();

            // From testing annealing does more bad than good. My explaination is that
            // in most cases the optimal result is found early and if it is not, annealing
            // make the algorithm less likely to find it.
            //annealingFactor = 1.0 - (1.0 / maxGeneration);
            /// TODO: Investigate the influence of this, as well as possible
            /// (automatic) parametrizations.
            temperature = 6.0;
        }

        /// <summary>
        ///  Generates populationSize random individuals.
        /// </summary>
        /// <returns>The random individuals.</returns>
        private Individual[] createPopulation()
        {
            Individual[] newPopulation = new Individual[populationSize];
            //for (int i = 0; i < populationSize; i++)
            Parallel.For(0, populationSize, i =>
            {
                newPopulation[i] = spawnIndividual(randomBitarray(dnaLength));
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
                    " generation without prior call to StartEvolution!");

            //List<Individual> newPopulation = new List<Individual>(populationSize);
            Individual[] newPopulation = new Individual[populationSize];
            int newPopIndex = 0;
            generationCount++;

            WeightedSampler<Individual> sampler = new WeightedSampler<Individual>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Check the fitness values of the current generation.
            //foreach (Individual individual in population)
            //{
            //    maxFitness = Math.Max(individual.Fitness, maxFitness);
            //    minFitness = Math.Min(individual.Fitness, minFitness);
            //}

            double averageHealth = 0;
            double averageBitsSet = 0;
            double averageAge = 0;
            int acceptedTotal = 0;
            int acceptedWorse = 0;
            //int purgedIndividuals = 0;

            // Sort the population by fitness.
            population = population.OrderBy(ind => ind.Fitness).ToArray();

            int index = 0;
            foreach (Individual individual in population)
            {
                index++;
                /// Max and min fitness might have changed, so we need to
                /// normalize the fitnesses again.
                //individual.normalizedFitness = normalizeFitness(individual.Fitness);

                averageHealth += 1500 - individual.Fitness;// individual.normalizedFitness;
                averageBitsSet += SetBits(individual.DNA);

                // Survival of the fittest (population was ordered by fitness above)
                if (index < 0.5 * populationSize)
                {
                    // Could be slightly more concise, I know.
                    //purgedIndividuals++;
                    continue;
                }

                /// This seems to have a good effect on convergence speed.
                /// By only allowing solutions that survived a round of culling
                /// to procreate, the solution quality is kept high.
                if (individual.Age >= 1)
                    sampler.AddEntry(individual, individual./*normalized*/Fitness);
                averageAge += individual.Age;

                individual.Age++;

                newPopulation[newPopIndex] = individual;
                newPopIndex++;
            }
            /*if (purgedIndividuals == 0)
                minFitness = minCurrentFitness;*/
            //for (int i = 0; i < newPopIndex; i++)
            Parallel.For(0, newPopIndex, i =>
            {
                Individual temp = newPopulation[i];
                Individual mutation = spawnIndividual(mutateDNA(temp.DNA));
                // -1 makes the average age converge to 0 way faster, so most individuals
                // get thrown out after only a few generations. Leads to infertile
                // populations. Not 100% sure that this is better, but it seems to be
                // more robust in exchange for more time needed on average for getting
                // the best solution. (which we have enough time for anyway)
                mutation.Age = temp.Age;// -1; // TODO: Investigate.
                if (acceptNewState(temp, mutation))
                {
                    //acceptedTotal++;
                    //if (mutation.Fitness < temp.Fitness)
                    //    acceptedWorse++;
                    temp = mutation;
                }
                newPopulation[i] = temp;
            });

            stopwatch.Stop();
            //Console.Write("Evaluation time for " + generationCount + " : ");
            //Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
            //Console.WriteLine("Temperature: " + temperature);
            //Console.WriteLine("Average health: " + averageHealth / populationSize);
            //Console.WriteLine("Average bits set: " + averageBitsSet / populationSize);
            //Console.WriteLine("Average age: " + averageAge / populationSize);
            //Console.WriteLine("Accepted new states (all/worse): " + acceptedTotal + "/" + acceptedWorse);
            //Console.WriteLine("Purged individuals: " + purgedIndividuals + "/" + populationSize);
            //Console.WriteLine("Sampler entries: " + sampler.EntryCount);
            
            stopwatch.Restart();

            if (!sampler.CanSample)
            {
                // This is actually a pretty serious problem.
                population = createPopulation();
                Console.WriteLine("Entire population was infertile (Generation " +
                                   generationCount + ").");
                //Debug.Fail("Population went extinct, not good...");
                return generationCount;
            }

            // Breed population and apply random mutations.
            //int dnaResets = 0;
            // Replace purged individuals
            //for (int i = newPopIndex; i < populationSize; i++)
            Parallel.For(newPopIndex, populationSize, i =>
            {
                BitArray parent1 = sampler.RandomSample().DNA;
                BitArray parent2 = sampler.RandomSample().DNA;

                BitArray newDNA = combineIndividualsDNA(parent1, parent2);

                //newPopulation.Add(spawnIndividual(newDNA));
                newPopulation[i] = spawnIndividual(newDNA);
            });

            population = newPopulation;//.ToArray();

            // Doing this at the end so the last generation has a use.
            updateBestSolution();

            // Yeah, I know, out of thin air.
            //temperature *= annealingFactor;

            stopwatch.Stop();
            //Console.WriteLine("Best value so far: " + (1500 - bestSolution.Fitness));
            //Console.WriteLine("------------------");
            //Console.Out.Flush();

            return generationCount;
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
            //individual.health = normalizeFitness(individual.Fitness);
            return individual;
        }

        /// <summary>
        ///  Maps fitness values into the 0 - 1 range, relative to the fitness
        ///  range of the encountered individuals.
        /// </summary>
        /// <param name="fitness">The fitness to be normalized.</param>
        /// <returns>A value between 0 and 1 (inclusive).</returns>
        //private double normalizeFitness(double fitness)
        //{
        //    if (maxFitness - minFitness <= 0) return 0.5; // What can you say...
        //    return (fitness - minFitness) / (maxFitness - minFitness);
        //}

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
        ///  the bits is set.
        /// </summary>
        /// <param name="length">The desired length of the BitArray.</param>
        /// <returns>The random BitArray.</returns>
        private BitArray randomBitarray(int length)
        {
            // Each byte provides 8 bits.
            /*byte[] buffer = new byte[(int)Math.Ceiling(length/8.0)];
            random.NextBytes(buffer);

            BitArray bitArray = new BitArray(buffer); //(buffer);
            bitArray.Length = length;
            return bitArray;*/

            if (length == 0)
                return new BitArray(0);

            BitArray bitArray = new BitArray(length);
            int i0 = random.Next(length);
            bitArray[i0] = true;
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

        #region Simulated Annealing
        private bool acceptNewState(Individual oldState, Individual newState)
        {
            double df = newState.Fitness - oldState.Fitness;
            if (df >= 0) return true;
            double acceptanceProbability = Math.Exp(df / temperature);
            if (random.NextDouble() < acceptanceProbability) return true;
            return false;
        }
        #endregion
    }
}
