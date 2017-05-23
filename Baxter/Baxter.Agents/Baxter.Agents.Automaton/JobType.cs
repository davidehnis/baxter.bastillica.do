using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Baxter.Agents.Automaton
{
    public enum JobType
    {
        // calculates and returns a value, list of parameters of the same type as return
        Calculation,

        // iterates through a collection and returns a value, no parameters
        Loop,

        // accepts parameters and returns a modified resource
        Resource,

        // CRUD operations on a cache or object set, cache parameter and returns altered cache
        CacheManipulation,

        // CRUD operations on an object or set of objects, object as parameter and returns altered
        // object graph
        ObjectManipulation,

        // Evaluations a large data set and returns results. Takes a data set as a parameter
        Learning,
    }
}