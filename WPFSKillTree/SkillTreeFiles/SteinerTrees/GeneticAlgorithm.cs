using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    /// Implements a genetic algorithm that uses bitstrings to represent solutions
    /// and attempts to find the lowest cost one through mutation and crossovers.
    /// </summary>
    /// <remarks>
    /// It is encouraged to adapt this code to the problem at hand. In particular,
    /// variable length DNA is not implemented and the mutation/crossover proba-
    /// bilities involved are best suited for certain shapes of fitness functions.
    /// Also see the NFL-theorem.
    /// </remarks>
    class GeneticAlgorithm
    {
        /// <summary>
        ///  The fitness function to be used for evaluating individuals.
        /// </summary>
        /// <param name="DNA">The bitstring encoding the solution to be
        /// evaluated.</param>
        /// <returns>The fitness of the DNA, a score for how good the
        /// corresponding solution is.</returns>
        public delegate double SolutionFitness(BitArray DNA);
        SolutionFitness solutionFitness;

        /// Asking for delegates to convert the bitstrings to the actual objects in
        /// here (and making this class generic) would be pretty silly in my eyes.

        List<Individual> population;

        int populationSize;

        Individual bestSolution;

        double maxFitness;
        double minFitness;

        int dnaLength;

        Random random;

        double degradationFactor;

        /// <summary>
        ///  An individual, comprised of a DNA and a fitness value, for use in
        ///  the genetic algorithm.
        /// </summary>
        class Individual
        {
            public BitArray DNA;
            public double fitness;
            public double health;
            public int age;

            public Individual(BitArray DNA, double fitness = 0)
            {
                this.DNA = DNA;
                this.fitness = fitness;
                this.age = 0;
            }
        }

        int generationCount;
        public int GenerationCount { get { return generationCount; } }

        /// <summary>
        /// Initializes a new instance of the genetic algorithm optimizer.
        /// </summary>
        /// <param name="solutionFitness">A delegate to the fitness function.</param>
        /// <param name="random">An optional Random instance to allow for seeding.
        /// If none is provided, a newly created one is used.</param>
        public GeneticAlgorithm(SolutionFitness solutionFitness, Random random = null)
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
        /// <param name="dnaLength">The (fixed) length of the DNA bitstrings used
        /// to encode solutions.</param>
        /// <param name="estimatedMaxFitness">An optional estimate of the maximum
        /// achievable fitness value of the fitness function. Better estimates may
        /// improve the convergence behavior by .</param>
        /// <param name="estimatedMinFitness">An optional estimate of the minimum
        /// achievable fitness value of the fitness function.</param>
        public void StartEvolution(int populationSize, int dnaLength,
            double estimatedMaxFitness = 0, double estimatedMinFitness = double.MaxValue)
            //BitArray initialSolution = null)
        {
            this.populationSize = populationSize;
            degradationFactor = 1.0 - (1.0 / populationSize);

            /// These assignments (based on the defaults) look paradoxical here,
            /// but they'll be adjusted by evaluating individuals before ever being
            /// used.
            this.maxFitness = estimatedMaxFitness;
            this.minFitness = estimatedMinFitness;

            this.dnaLength = dnaLength;

            bestSolution = new Individual(null, 0);

            population = createPopulation();
            generationCount = 0;
        }

        private List<Individual> createPopulation()
        {
            List<Individual> newPopulation = new List<Individual>();
            for (int i = 0; i < populationSize; i++)
            {
                Individual individual = new Individual(randomBitarray(dnaLength));
                newPopulation.Add(individual);
            }

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

            List<Individual> newPopulation = new List<Individual>();
            generationCount++;

            WeightedSampler<Individual> sampler = new WeightedSampler<Individual>();


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double minCurrentFitness = double.MaxValue;
            double maxCurrentFitness = double.MinValue;


            // Evaluate all new individuals
            foreach (Individual individual in population)
            {
                if (individual.age == 0)
                    individual.fitness = solutionFitness(individual.DNA);

                if (individual.fitness < 0)
                    throw new ArgumentOutOfRangeException("solutionFitness function",
                        "Negative fitness values are not allowed! Use 0 fitness " +
                        "for solutions that should not reproduce.");

                if (individual.fitness > bestSolution.fitness)
                    bestSolution = new Individual(individual.DNA, individual.fitness);

                // TODO: Treat 0 fitness special?
                maxFitness = Math.Max(individual.fitness, maxFitness);
                minFitness = Math.Min(individual.fitness, minFitness);

                maxCurrentFitness = Math.Max(individual.fitness, maxCurrentFitness);
                minCurrentFitness = Math.Min(individual.fitness, minCurrentFitness);
            }

            double averageHealth = 0;
            double averageBitsSet = 0;
            double averageAge = 0;
            int maxFitnessCount = 0;
            int purgedIndividuals = 0;

            population = population.OrderBy(ind => ind.fitness).ToList();

            foreach (Individual individual in population)
            {
                if (individual.age == 0)
                    individual.health = normalizeFitness(individual.fitness);
                else
                    individual.health *= degradationFactor;

                averageHealth += individual.health;
                averageBitsSet += SetBits(individual.DNA);
                if (individual.health == 1) maxFitnessCount++;

                if (individual.health >= 0.5)
                {
                    individual.age++;
                    //individual.DNA = mutateDNA(individual.DNA);
                    averageAge += individual.age;
                    newPopulation.Add(individual);
                    sampler.AddEntry(individual, individual.health);
                }
                else purgedIndividuals++;
            }
            /*if (purgedIndividuals == 0)
                minFitness = minCurrentFitness;*/

            stopwatch.Stop();
            Console.Write("Evaluation time for " + generationCount + " : ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Average health: " + averageHealth / populationSize);
            //Console.WriteLine("Average bits set: " + averageBitsSet / populationSize);
            Console.WriteLine("Average age: " + averageAge / populationSize);
            Console.WriteLine("Max-fitness count: " + maxFitnessCount);
            Console.WriteLine("Purged individuals: " + purgedIndividuals + "/" + populationSize);
            if (minCurrentFitness == maxCurrentFitness)
                Console.WriteLine("Entire population had the same fitness value.");

            stopwatch.Restart();

            if (!sampler.CanSample)
            {
                population = createPopulation();
                Console.WriteLine("Entire population was infertile (Generation " +
                                   generationCount + ").");
                return generationCount;
            }

            // Breed population and apply random mutations.
            int dnaResets = 0;
            // Replace purged individuals
            for (int i = 0; i < purgedIndividuals; i++)
            {
                BitArray parent1 = sampler.RandomSample().DNA;
                BitArray parent2 = sampler.RandomSample().DNA;

                Individual newIndividual = new Individual(combineIndividualsDNA(parent1, parent2));

                newIndividual.DNA = mutateDNA(newIndividual.DNA);

                newPopulation.Add(newIndividual);
            }

            population = newPopulation;

            stopwatch.Stop();
            //Console.Write("Mutation time for " + generationCount + " : ");
            //Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("Best value so far: " + (1500 - bestSolution.fitness));
            Console.WriteLine("------------------");
            Console.Out.Flush();

            return generationCount;
        }

        public BitArray GetBestDNA()
        {
            //Console.WriteLine("Returning DNA with fitness " + bestSolution.fitness);
            return new BitArray(bestSolution.DNA);
        }

        // Maps fitness values into the 0 - 1 range.
        private double normalizeFitness(double fitness)
        {
            if (maxFitness - minFitness <= 0) return 0.5; // What can you say...
            return (fitness - minFitness) / (maxFitness - minFitness);
        }

        private BitArray mutateDNA(BitArray dna)
        {
            /// Chance for each individual bit to be mutated. Not currently used
            /// since only one bit is mutated at a time anyway.
            //double mutationProbability = 1 - Math.Sqrt(normalizeFitness(individual.fitness));

            /*BitArray newDNA = new BitArray(individual.DNA);
            BitArray mutationSequence = randomBitarray(newDNA.Length);

            for (int i = 0; i < newDNA.Length; i++)
            {
                //if (random.NextDouble() < mutationProbability)
                newDNA[i] = (mutationSequence[i] ? !newDNA[i] : newDNA[i]);
            }

            return newDNA;*/
            BitArray newDNA = new BitArray(dna);
            int index = random.Next(newDNA.Length);
            newDNA[index] = !newDNA[index];
            return newDNA;
        }

        private BitArray combineIndividualsDNA(BitArray dna1, BitArray dna2)
        {
            int length = dna1.Length;
            if (dna2.Length != length)
                throw new NotImplementedException("Breeding of individuals with" +
                            "differing DNA lengths is not yet implemented!");

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

        private BitArray crossoverDNA(BitArray DNA1, BitArray DNA2, int start, int end)
        {
            BitArray cross = new BitArray(DNA1);
            for (int i = start; i < end; i++)
                cross[i] = DNA2[i];
            return cross;
        }


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

            // Returns a bit array filled with 0s except for one spot.
            BitArray bitArray = new BitArray(length);
            int i0 = random.Next(length);
            bitArray[i0] = true;
            return bitArray;
        }

        public static int SetBits(BitArray dna)
        {
            int sum = 0;
            for (int i = 0; i < dna.Length; i++)
                sum += (dna[i] ? 1 : 0);
            return sum;
        }
    }
}
