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
    class GeneticAlgorithm
    {
        public delegate double SolutionCost(BitArray individual);
        SolutionCost solutionCost;
        
        List<BitArray> population;
        int populationSize;

        BitArray bestSolution;
        double lowestCost;

        double estimatedMinCost;

        double thisAlgorithmBecomingSkynetCost = 999999999;

        public GeneticAlgorithm(SolutionCost solutionCost)
        {
            this.solutionCost = solutionCost;
            
            lowestCost = double.PositiveInfinity;
        }


        public void StartEvolution(int populationSize, double estimatedMinCost = 0, BitArray initialSolution = null)
        {
            this.populationSize = populationSize;
        }

    
        public void NewGeneration()
        {
            List<BitArray> newPopulation;

            TreeSampler<BitArray> sampler = new TreeSampler<BitArray>();

            // TODO: Generate new population
            foreach (BitArray individual in population)
            {
                double cost = solutionCost(individual);
                sampler.AddEntry(individual, cost);
            }

        }


        private BitArray Mutate(BitArray individual, double cost)
        {
            throw new NotImplementedException();
        }


        // Good cost function
    }
}
