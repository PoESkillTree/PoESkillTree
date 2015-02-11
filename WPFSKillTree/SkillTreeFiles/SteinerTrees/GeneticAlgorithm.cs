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

        /// <summary>
        ///  An individual, comprised of a DNA and a fitness value, for use in
        ///  the genetic algorithm.
        /// </summary>
        class Individual
        {
            public BitArray DNA;
            public double fitness;

            public Individual(BitArray DNA, double fitness = 0)
            {
                this.DNA = DNA;
                this.fitness = fitness;
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
        /// Progresses the optimization by evolving the current population.
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
            // Evaluate all individuals
            foreach (Individual individual in population)
            {
                double fitness = solutionFitness(individual.DNA);
                if (fitness < 0)
                    throw new ArgumentOutOfRangeException("solutionFitness function",
                        "Negative fitness values are not allowed! Use 0 fitness " +
                        "for solutions that should not reproduce.");

                individual.fitness = fitness;
                sampler.AddEntry(individual, fitness);

                if (fitness > bestSolution.fitness)
                    bestSolution = individual;

                if (fitness > maxFitness)
                    maxFitness = fitness;
                    // TODO: Treat 0 fitness special?
                else if (fitness < minFitness)
                    minFitness = fitness;
            }
            stopwatch.Stop();
            Console.Write("Evaluation time for " + generationCount + " : ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");


            stopwatch.Restart();

            if (!sampler.CanSample)
            {
                population = createPopulation();
                Console.WriteLine("Entire population was infertile (Generation " +
                                   generationCount + ").");
            }

            // Mutate based on fitness
            for (int i = 0; i < populationSize; i++)
            {
                Individual parent = sampler.RandomSample();

                Individual mutation = new Individual(mutateDNA(parent));
                newPopulation.Add(mutation);
            }

            // Apply DNA crossover
            foreach (Individual individual in newPopulation)
            {
                Individual partner = sampler.RandomSample();
                /// The higher the relative fitness of the partner, the more
                /// likely DNA crossover is allowed.
                /// Note: This likely has an effect on (premature) convergence.
                if (random.NextDouble() <
                    partner.fitness / (partner.fitness + individual.fitness))
                    combineIndividuals(individual, partner);
            }

            population = newPopulation;

            stopwatch.Stop();
            Console.Write("Mutation time for " + generationCount + " : ");
            Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");

            Console.WriteLine("Best value so far: " + 1 / bestSolution.fitness);

            return generationCount;
        }

        public BitArray BestDNA()
        {
            return new BitArray(bestSolution.DNA);
        }

        // Maps fitness values into the 0 - 1 range.
        private double normalizeFitness(double fitness)
        {
            return (fitness - minFitness) / (maxFitness - minFitness);
        }

        private BitArray mutateDNA(Individual individual)
        {
            // Chance for each individual bit to be mutated.
            double mutationProbability = 1 - Math.Sqrt(normalizeFitness(individual.fitness));

            BitArray newDNA = new BitArray(individual.DNA);
            BitArray mutationSequence = randomBitarray(newDNA.Length);

            for (int i = 0; i < newDNA.Length; i++)
            {
                if (random.NextDouble() < mutationProbability)
                    newDNA[i] = mutationSequence[i];
            }

            return newDNA;
        }

        private BitArray combineIndividuals(Individual parent1, Individual parent2)
        {
            int length = parent1.DNA.Length;
            if (parent2.DNA.Length != length)
                throw new NotImplementedException("Breeding of individuals with" +
                            "differing DNA lengths is not yet implemented!");

            int crossoverStart = random.Next(length);
            int crossoverEnd   = random.Next(length);

            // This prevents the crossover being biased towards exchanging
            // the middle parts of the DNA and basically never affecting the
            // start or end of it.
            if (crossoverStart > crossoverEnd)
                return crossoverDNA(parent2.DNA, parent1.DNA,
                                    crossoverEnd, crossoverStart);
            else
                return crossoverDNA(parent1.DNA, parent2.DNA,
                                    crossoverStart, crossoverEnd);
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
            byte[] buffer = new byte[(int)Math.Ceiling(length/8.0)];
            random.NextBytes(buffer);

            BitArray bitArray = new BitArray(buffer);
            bitArray.Length = length;
            return bitArray;
        }
    }
}
