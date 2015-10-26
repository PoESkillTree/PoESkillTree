using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Data struct to pass parameters to <see cref="GeneticAlgorithm"/>.
    /// Most of these are up for experimentation to see what produces the best results.
    /// </summary>
    public struct GeneticAlgorithmParameters
    {
        /// <summary>
        /// The number of generations the GA should calculate.
        /// </summary>
        public readonly int MaxGeneration;

        /// <summary>
        /// The number of individuals kept in the population simultaneously.
        /// </summary>
        public readonly int PopulationSize;

        /// <summary>
        /// The length of the dna of individuals (number of bools representing the dna).
        /// </summary>
        public readonly int DnaLength;

        /// <summary>
        /// Factor describing how often worse mutated individuals will replace the original.
        /// Higher equals more often.
        /// </summary>
        public readonly double Temperature;

        /// <summary>
        /// Factor by which the the temperature should be multiplied after each generation.
        /// </summary>
        public readonly double AnnealingFactor;

        /// <summary>
        /// Maximum length of sequences that are flipped at once by mutation.
        /// </summary>
        public readonly int MaxMutateClusterSize;

        public GeneticAlgorithmParameters(int maxGeneration, int populationSize, int dnaLength,
            double temperature = 6, double annealingFactor = 1, int maxMutateClusterSize = 1)
        {
            if (maxGeneration < 0)
                throw new ArgumentOutOfRangeException("maxGeneration", maxGeneration, "must be >= 0");
            if (populationSize < 0)
                throw new ArgumentOutOfRangeException("populationSize", populationSize, "must be <= 0");
            if (dnaLength < 0)
                throw new ArgumentOutOfRangeException("dnaLength", dnaLength, "must be >= 0");
            if (maxMutateClusterSize < 1)
                throw new ArgumentOutOfRangeException("maxMutateClusterSize", maxMutateClusterSize, "must be > 0");

            MaxGeneration = maxGeneration;
            PopulationSize = populationSize;
            DnaLength = dnaLength;
            Temperature = temperature;
            AnnealingFactor = annealingFactor;
            MaxMutateClusterSize = maxMutateClusterSize;
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
        /// <param name="dna">The bitstring encoding the solution to be
        /// evaluated.</param>
        /// <returns>The fitness of the DNA, a score for how good the
        /// corresponding solution is.</returns>
        public delegate double SolutionFitnessFunction(BitArray dna);

        private readonly SolutionFitnessFunction _solutionFitness;

        /// Asking for delegates to convert the bitstrings to the actual objects in
        /// here (and making this class generic) would be pretty silly in my eyes.

        private int _dnaLength;

        private Individual[] _population;

        private int _populationSize;

        private double _temperature;

        private double _annealingFactor;

        public int GenerationCount { get; private set; }

        public int MaxGeneration { get; private set; }

        private BitArray _initialSolution;

        private Individual _bestSolution;

        private int _maxMutateClusterSize;

        /// <summary>
        ///  Retrieves the DNA with the highest encountered fitness value thus far.
        /// </summary>
        /// <returns>The DNA holding the current fitness record.</returns>
        public BitArray GetBestDNA()
        {
            return new BitArray(_bestSolution.DNA);
        }
        
        private readonly Random _random = new ThreadSafeRandom();

        /// <summary>
        ///  An individual, comprised of a DNA and a fitness value, for use in
        ///  the genetic algorithm.
        /// </summary>
        private class Individual
        {
            public readonly BitArray DNA;
            
            public readonly double Fitness;

            public int Rank;

            // The amount of generations this individual has lived.
            public int Age;

            public Individual(BitArray dna, double fitness)
            {
                DNA = dna;
                Fitness = fitness;
                Age = 0;
            }
        }

        /// <summary>
        /// Cache for the fitness values so they are only calculated once.
        /// </summary>
        private readonly ConcurrentDictionary<BitArrayKey, double> _fitnessCache = new ConcurrentDictionary<BitArrayKey, double>();

        /// <summary>
        /// Initializes a new instance of the genetic algorithm optimizer.
        /// </summary>
        /// <param name="solutionFitness">A delegate to the fitness function.
        /// Because of parallelization the fitness function must be thread safe</param>
        public GeneticAlgorithm(SolutionFitnessFunction solutionFitness)
        {
            // Save the fitness function
            _solutionFitness = solutionFitness;
        }

        /// <summary>
        /// Initializes a new optimization run.
        /// </summary>
        /// <param name="parameters">The parameters to initialize the algorithm with</param>
        /// <param name="initialSolution">The solution to initialize the population with</param>
        public void InitializeEvolution(GeneticAlgorithmParameters parameters, BitArray initialSolution = null)
        {
            _populationSize = parameters.PopulationSize;
            MaxGeneration = parameters.MaxGeneration;
            _dnaLength = parameters.DnaLength;
            _temperature = parameters.Temperature;
            _annealingFactor = parameters.AnnealingFactor;
            _maxMutateClusterSize = parameters.MaxMutateClusterSize;

            _bestSolution = new Individual(null, 0);

            _initialSolution = initialSolution ?? new BitArray(_dnaLength);
            _population = CreatePopulation();
            GenerationCount = 0;
            UpdateBestSolution();
        }

        /// <summary>
        ///  Generates populationSize random individuals.
        /// </summary>
        /// <returns>The random individuals.</returns>
        private Individual[] CreatePopulation()
        {
            if (_populationSize == 0)
            {
                return new Individual[0];
            }

            Individual[] newPopulation = new Individual[_populationSize];
            // The initial solution is included in the initial population.
            newPopulation[0] = SpawnIndividual(_initialSolution);
            newPopulation[0].Age++;
            //for (int i = 1; i < populationSize; i++)
            Parallel.For(1, _populationSize, i =>
            {
                newPopulation[i] = SpawnIndividual(RandomBitarray());
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
            if (_population == null)
                throw new InvalidOperationException("Cannot generate a next" +
                    " generation without prior call to InitializeEvolution!");

            Individual[] newPopulation = new Individual[_populationSize];
            int newPopIndex = 0;
            GenerationCount++;

            WeightedSampler<Individual> sampler = new WeightedSampler<Individual>(_random);

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
            _population = _population.OrderBy(ind => ind.Fitness).ToArray();

            int index = 0;
            foreach (Individual individual in _population)
            {
                index++;

#if DEBUG
                averageHealth += individual.Fitness;
                averageBitsSet += SetBits(individual.DNA);
#endif

                // Survival of the fittest (population was ordered by fitness above)
                if (index < 0.5 * _populationSize)
                {
                    continue;
                }

                // This seems to have a good effect on convergence speed.
                // By only allowing solutions that survived a round of culling
                // to procreate, the solution quality is kept high.
                if (individual.Age >= 1)
                    sampler.AddEntry(individual, index);
#if DEBUG
                averageAge += individual.Age;
#endif
                individual.Rank = index;
                individual.Age++;

                newPopulation[newPopIndex++] = individual;
            }

            //for (int i = 0; i < newPopIndex; i++)
            Parallel.For(0, newPopIndex, i =>
            {
                Individual temp = newPopulation[i];
                Individual mutation = SpawnIndividual(MutateDNA(temp.DNA));

                // Lowering the age here would lead to faster convergence but would
                // make the population go extinct several times.
                mutation.Age = temp.Age;
                // Mutations have a chance to be rejected based on the fitness loss
                // relative to the non-mutated individual. See explanation above.
                if (AcceptNewState(temp, mutation))
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
            //Debug.Write("Evaluation time for " + GenerationCount + " : ");
            //Debug.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
            //Debug.WriteLine("Average health: " + averageHealth / _populationSize);
            //Debug.WriteLine("Average bits set: " + averageBitsSet / _populationSize);
            //Debug.WriteLine("Average age: " + averageAge / PopulationSize);
            //Debug.WriteLine("Accepted new states (all/worse): " + acceptedTotal + "/" + acceptedWorse);
            //Debug.WriteLine("Sampler entries: " + sampler.EntryCount);
            
            stopwatch.Restart();
#endif

            if (!sampler.CanSample)
            {
                // This is actually a pretty serious problem.
                _population = CreatePopulation();
                Debug.WriteLine("Entire population was infertile (Generation " +
                                   GenerationCount + ").");
                //Debug.Fail("Population went extinct, not good...");
                return GenerationCount;
            }

            // Replace purged individuals
            //for (int i = newPopIndex; i < populationSize; i++)
            Parallel.For(newPopIndex, _populationSize, i =>
            {
                BitArray parent1 = sampler.RandomSample().DNA;
                BitArray parent2 = sampler.RandomSample().DNA;

                BitArray newDNA = CombineIndividualsDNA(parent1, parent2);

                newPopulation[i] = SpawnIndividual(newDNA);
            });

            _population = newPopulation;

            // Doing this at the end so the last generation has a use.
            UpdateBestSolution();

            _temperature *= _annealingFactor;

#if DEBUG
            stopwatch.Stop();
            //Debug.WriteLine("Best value so far: " + _bestSolution.Fitness);
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
        private void UpdateBestSolution()
        {
            foreach (Individual individual in _population)
            {
                if (individual.Fitness < 0)
                    throw new ArgumentOutOfRangeException("solutionFitness function",
                        "Negative fitness values are not allowed! Use 0 fitness " +
                        "for solutions that should not reproduce.");

                if (individual.Fitness > _bestSolution.Fitness)
                    _bestSolution = new Individual(individual.DNA, individual.Fitness);
            }
        }

        /// <summary>
        ///  Factory method for generating a new individual from a DNA. Passing the
        ///  fitness function to every individual is weird, so that gets done here.
        ///  Fitness values are cached so they are only calculated once.
        /// </summary>
        /// <param name="dna">The DNA of the new individual.</param>
        /// <returns>The new individual.</returns>
        private Individual SpawnIndividual(BitArray dna)
        {
            var fitness = _fitnessCache.GetOrAdd(new BitArrayKey(dna), key => _solutionFitness(key.Data));
            return new Individual(dna, fitness);
        }

        #region DNA mutation
        /// <summary>
        ///  Flips a random sequence of bits in the passed DNA bitstring and returns the result.
        ///  The sequence length is up to _maxMutateClusterSize (inclusive).
        /// </summary>
        /// <param name="dna">The DNA to be mutated.</param>
        /// <returns>The mutated DNA.</returns>
        private BitArray MutateDNA(BitArray dna)
        {
            BitArray newDNA = new BitArray(dna);
            var index = _random.Next(newDNA.Length);
            var count = _random.Next(_maxMutateClusterSize) + 1;
            for (var i = index; i < (count + index) && i < _dnaLength; i++)
            {
                newDNA[i] = !newDNA[i];
            }
            return newDNA;
        }

        /// <summary>
        ///  Creates a new DNA bitstring from two input ones. A (random) sequence
        ///  of one input DNA is "filled up" with the data of the other one.
        /// </summary>
        /// <param name="dna1">The first input DNA.</param>
        /// <param name="dna2">The second input DNA:</param>
        /// <returns>The combined DNA.</returns>
        private BitArray CombineIndividualsDNA(BitArray dna1, BitArray dna2)
        {
            int length = dna1.Length;
            if (dna2.Length != length)
                throw new NotSupportedException("Breeding of individuals with" +
                            " differing DNA lengths is not supported by this GeneticAlgorithm!");

            int crossoverStart = _random.Next(length);
            int crossoverEnd   = _random.Next(length);

            // This prevents the crossover being biased towards exchanging
            // the middle parts of the DNA and basically never affecting the
            // start or end of it.
            if (crossoverStart > crossoverEnd)
                return CrossoverDNA(dna2, dna1, crossoverEnd, crossoverStart);
            else
                return CrossoverDNA(dna1, dna2, crossoverStart, crossoverEnd);
        }

        /// <summary>
        ///  Replaces the bits in DNA1 from start to end with those from DNA2 and
        ///  returns the result.
        /// </summary>
        /// <param name="dna1">The "base" DNA.</param>
        /// <param name="dna2">The "overwriting" DNA.</param>
        /// <param name="start">The index of the start of the DNA exchange.</param>
        /// <param name="end">The index of the end of the DNA exchange.</param>
        /// <returns></returns>
        private BitArray CrossoverDNA(BitArray dna1, BitArray dna2, int start, int end)
        {
            BitArray cross = new BitArray(dna1);
            for (int i = start; i <= end; i++)
                cross[i] = dna2[i];
            return cross;
        }

        /// <summary>
        ///  Generates a random BitArray. Currently this means that exactly one of
        ///  the bits is inverted from the initial solution.
        /// </summary>
        /// <returns>The random BitArray.</returns>
        private BitArray RandomBitarray()
        {
            BitArray bitArray = new BitArray(_initialSolution);
            int i0 = _random.Next(_dnaLength);
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
        private bool AcceptNewState(Individual oldState, Individual newState)
        {
            var curFitness = oldState.Fitness;
            var newFitness = newState.Fitness;
            if (newFitness >= curFitness) return true;
            
            int i, imin = 0, imax = oldState.Rank - 1;
            for (i = (imin + imax) / 2; i < imax; i = (imin + imax) / 2)
            {
                curFitness = _population[i].Fitness;
                if (curFitness < newFitness)
                    imin = i + 1;
                else
                    imax = i - 1;
            }
            // Above search either returns the correct index or is the correct index - 1.
            if (_population[i].Fitness < newFitness) i++;
            var df = i - oldState.Rank;
            
            double acceptanceProbability = Math.Exp(df / _temperature);
            return _random.NextDouble() < acceptanceProbability;
        }
    }
}
