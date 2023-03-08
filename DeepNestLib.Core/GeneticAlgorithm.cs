using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepNestLib
{
    public class GeneticAlgorithm
    {
        SvgNestConfig Config;
        public List<PopulationItem> population;

        public static bool StrictAngles = false;
        float[] defaultAngles = new float[] {
        0,
0,
90,
0,
0,
270,
180,
180,
180,
90

        };
                
        public GeneticAlgorithm(NFP[] adam, SvgNestConfig config)
        {
            List<float> ang2 = new List<float>();
            for (int i = 0; i < adam.Length; i++)
            {
                ang2.Add((i * 90) % 360);
            }
            defaultAngles = ang2.ToArray();
            Config = config;


            List<float> angles = new List<float>();
            for (int i = 0; i < adam.Length; i++)
            {
                if (StrictAngles)
                {
                    angles.Add(defaultAngles[i]);
                }
                else
                {
                    var angle = (float)Math.Floor(r.NextDouble() * Config.rotations) * (360f / Config.rotations);
                    angles.Add(angle);
                }

                //angles.Add(randomAngle(adam[i]));
            }


            population = new List<PopulationItem>();
            population.Add(new PopulationItem() { placements = adam.ToList(), Rotation = angles.ToArray() });            
            while (population.Count() < config.populationSize)
            {
                var mutant = this.mutate(population[0]);
                population.Add(mutant);
            }
        }

        public PopulationItem mutate(PopulationItem p)
        {
            var clone = new PopulationItem();

            clone.placements = p.placements.ToArray().ToList();
            clone.Rotation = p.Rotation.Clone() as float[];
            for (int i = 0; i < clone.placements.Count(); i++)
            {
                var rand = r.NextDouble();
                if (rand < 0.01 * Config.mutationRate)
                {
                    var j = i + 1;
                    if (j < clone.placements.Count)
                    {
                        var temp = clone.placements[i];
                        clone.placements[i] = clone.placements[j];
                        clone.placements[j] = temp;
                    }
                }
                rand = r.NextDouble();
                if (rand < 0.01 * Config.mutationRate)
                {
                    clone.Rotation[i] = (float)Math.Floor(r.NextDouble() * Config.rotations) * (360f / Config.rotations);
                }
            }


            return clone;
        }
        Random r = new Random();
        public float[] shuffleArray(float[] array)
        {
            for (var i = array.Length - 1; i > 0; i--)
            {
                var j = (int)Math.Floor(r.NextDouble() * (i + 1));
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
            return array;
        }


        // returns a random individual from the population, weighted to the front of the list (lower fitness value is more likely to be selected)
        public PopulationItem randomWeightedIndividual(PopulationItem exclude = null)
        {
            //var pop = this.population.slice(0);
            var pop = this.population.ToArray();

            if (exclude != null && Array.IndexOf(pop, exclude) >= 0)
            {
                pop.splice(Array.IndexOf(pop, exclude), 1);
            }

            var rand = r.NextDouble();

            float lower = 0;
            var weight = 1 / (float)pop.Length;
            float upper = weight;

            for (var i = 0; i < pop.Length; i++)
            {
                // if the random number falls between lower and upper bounds, select this individual
                if (rand > lower && rand < upper)
                {
                    return pop[i];
                }
                lower = upper;
                upper += 2 * weight * ((pop.Length - i) / (float)pop.Length);
            }

            return pop[0];
        }

        // single point crossover
        public PopulationItem[] mate(PopulationItem male, PopulationItem female)
        {
            var cutpoint = (int)Math.Round(Math.Min(Math.Max(r.NextDouble(), 0.1), 0.9) * (male.placements.Count - 1));

            var gene1 = new List<NFP>(male.placements.Take(cutpoint).ToArray());
            var rot1 = new List<float>(male.Rotation.Take(cutpoint).ToArray());

            var gene2 = new List<NFP>(female.placements.Take(cutpoint).ToArray());
            var rot2 = new List<float>(female.Rotation.Take(cutpoint).ToArray());

            int i = 0;

            for (i = 0; i < female.placements.Count; i++)
            {
                if (!gene1.Any(z => z.id == female.placements[i].id))
                {
                    gene1.Add(female.placements[i]);
                    rot1.Add(female.Rotation[i]);
                }
            }

            for (i = 0; i < male.placements.Count; i++)
            {
                if (!gene2.Any(z => z.id == male.placements[i].id))
                {
                    gene2.Add(male.placements[i]);
                    rot2.Add(male.Rotation[i]);
                }
            }

            return new[] {new  PopulationItem() {
                placements= gene1, Rotation= rot1.ToArray()},
                new PopulationItem(){ placements= gene2, Rotation= rot2.ToArray()}};
        }

        public void generation()
        {
            // Individuals with higher fitness are more likely to be selected for mating
            population = population.OrderBy(z => z.fitness).ToList();

            // fittest individual is preserved in the new generation (elitism)

            List<PopulationItem> newpopulation = new List<PopulationItem>();
            newpopulation.Add(this.population[0]);
            while (newpopulation.Count() < this.population.Count)
            {
                var male = randomWeightedIndividual();
                var female = randomWeightedIndividual(male);

                // each mating produces two children
                var children = mate(male, female);

                // slightly mutate children
                newpopulation.Add(this.mutate(children[0]));

                if (newpopulation.Count < this.population.Count)
                {
                    newpopulation.Add(this.mutate(children[1]));
                }
            }

            this.population = newpopulation;
        }
    }
}