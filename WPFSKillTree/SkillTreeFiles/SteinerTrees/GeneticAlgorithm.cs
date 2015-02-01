using System;
using System.Collections;
using System.Collections.Generic;
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
        /// achievable fitness value of the cost function. Better estimates may
        /// improve the convergence behavior.</param>
        public void StartEvolution(int populationSize, int dnaLength, double estimatedMaxFitness = 0)
            //BitArray initialSolution = null)
        {
            this.populationSize = populationSize;

            // These assignments look paradoxical here, but they'll be adjusted
            // by evaluating individuals before ever being used.
            this.maxFitness = estimatedMaxFitness;
            this.minFitness = double.MaxValue;

            this.dnaLength = dnaLength;

            population = createPopulation();
            generationCount = 0;
        }

        private List<Individual> createPopulation()
        {
            List<Individual> newPopulation = new List<Individual>();
            for (int i = 0; i < populationSize; i++)
            {
                // TODO: DNA size
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

            // Evaluate all individuals
            foreach (Individual individual in population)
            {
                double fitness = solutionFitness(individual.DNA);
                if (fitness < 0)
                    throw new ArgumentOutOfRangeException("solutionFitness function",
                        "Negative fitness values are not allowed! Use 0 fitness" +
                        "for solutions that should not reproduce.");

                sampler.AddEntry(individual, fitness);

                if (fitness > bestSolution.fitness)
                    bestSolution = individual;
                if (fitness > maxFitness)
                    maxFitness = fitness;
            }

            if (!sampler.CanSample)
            {
                population = createPopulation();
                Console.WriteLine("Entire population was infertile (Generation " +
                                   generationCount + ")");
            }

            // Mutate based on fitness
            for (int i = 0; i < populationSize; i++)
            {
                Individual parent1 = sampler.RandomSample();

                Individual mutation = new Individual(mutateDNA(parent1));
                newPopulation.Add(mutation);
            }

            // TODO: Apply crossover

            population = newPopulation;
            return generationCount;
        }

        private double normalizeFitness(double fitness)
        {
            // Could incorporate something like a min fitness, in case it's
            // far away from 0.
            return fitness / maxFitness;
        }


        private BitArray mutateDNA(Individual individual)
        {
            // Normalized fitness will never be > 1.
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

        private BitArray breedIndividuals(Individual parent1, Individual parent2)
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
