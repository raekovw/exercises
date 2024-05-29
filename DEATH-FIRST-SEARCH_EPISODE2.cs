using System;
using System.Linq;
using System.Collections.Generic;

static class Input
{
    public static readonly int nbNode;
    public static readonly int nbLink;
    public static readonly int nbExit;

    static Input()
    {
        int[] nbElement = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
        nbNode = nbElement[0];
        nbLink = nbElement[1];
        nbExit = nbElement[2];
    }
}

class Network
{
    public List<int>[] Graph {get;} = new List<int>[Input.nbNode];

    public Network()
    {
        for (int i = 0; i < Input.nbNode; i++)
        {
            Graph[i] = new List<int>();
        }
    }

    public void Build()
    {
        for (int i = 0; i < Input.nbLink; i++)
        {
            int[] nodes = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
            int node1 = nodes[0];
            int node2 = nodes[1];
            Graph[node1].Add(node2);
            Graph[node2].Add(node1);
        }    
    }       
}

class Exit
{
    public List<int> Exits {get;} = new List<int>();

    public Exit(){}

    public void EnlistExits()
    {
        for (int i = 0; i < Input.nbExit; i++)
        {
            int exit = int.Parse(Console.ReadLine());
            Exits.Add(exit);
        } 
    }  
}

class Crit
{
    private List<int>[] Graph;
    private List<int> Exits;
    public Dictionary<int, List<int>> CritNode {get;} = new Dictionary<int, List<int>>(); 

    public Crit(Network network, Exit exit)
    {
       Graph = network.Graph;
       Exits = exit.Exits;
    }

    public void FindCritNode()
    {
        for(int i = 0; i < Input.nbNode; i++)
        {   
            List<int> curList = Graph[i];
            List<int> exitInRange = curList.Where(exit => Exits.Contains(exit)).ToList();
            bool isCritNode = exitInRange.Count == 2;

            if(isCritNode){
                CritNode[i] = new List<int>();
                CritNode[i].AddRange(exitInRange);
            }
        }
    }  
}

class CollectionHandler
{
    public List<int>[] Graph {get;}
    public List<int> Exits {get;}
    public Dictionary<int, List<int>> CritNode {get;}

    public CollectionHandler(Network network, Exit exit, Crit crit)
    {
        Graph = network.Graph;
        Exits = exit.Exits;
        CritNode = crit.CritNode;
    }

    private void RemoveLinkFromGraph(int node, int exit)
    {
        Graph[node].Remove(exit);
    } 

    private void RemoveCritNode(int critNode)
    {
        CritNode.Remove(critNode); 
    }                          

    public void RemoveLink(GameOutput output)
    { 
        RemoveLinkFromGraph(output.Node, output.Exit);  

        if(output.IsCrit){
           RemoveCritNode(output.Node);
        }
    }
    // public void remove
}

abstract class SeverLogic 
{
    protected List<int>[] graph;
    protected List<int> exits;
    protected Dictionary<int, List<int>> CritNode;
  
    public SeverLogic(CollectionHandler collection)
    {
        graph = collection.Graph;
        exits = collection.Exits;
        CritNode = collection.CritNode;
    }
}

class SmartSearch : SeverLogic
{
    public SmartSearch(CollectionHandler collection) : base(collection){}

    public GameOutput SearchSpecialNode(int bobPos)
    {
        return SearchEmergencyNode(bobPos);
    }

    private GameOutput SearchEmergencyNode(int bobPos)
    {
        int node = bobPos;
        List<int> Node1 = graph[node];

        foreach(int neighbor in Node1) 
        {
            if(exits.Contains(neighbor)){
                int exit = neighbor;
                return new GameOutput(node, exit);
            }
        } 
          
        return SearchCritNode(Node1); 
    }

    private GameOutput SearchCritNode(List<int> Node1) 
    {
        foreach (var neighbor in Node1) 
        {  
            if(CritNode.ContainsKey(neighbor)){
                int critNode = neighbor;
                int critExit = CritNode[neighbor][0];
                return new GameOutput(critNode, critExit, true);
            }
        } 

        return SearchSneakyCritNode(Node1);
    }

    public GameOutput SearchSneakyCritNode(List<int> Node1)
    {
        Stack<int> stack = new Stack<int>(Node1);
        bool[] visited = new bool[Input.nbNode];
        
        while (stack.Count != 0){
            int node = stack.Pop();
            visited[node] = true;

            foreach (int neighbor in graph[node]){   
                if(!visited[neighbor] && !exits.Contains(neighbor))
                {
                    if(CritNode.ContainsKey(neighbor)){
                        int critNode = neighbor;
                        int critExit = CritNode[neighbor][0];
                        return new GameOutput(critNode, critExit, true);
                    }

                    else{
                        stack.Push(neighbor);
                        visited[neighbor] = true;
                    }
                }                                
            }
        }

        return null;
    }
}

class RandomSearch : SeverLogic
{
    public RandomSearch(CollectionHandler collection) : base(collection){}

    public GameOutput BFS(int bobNode)
    {
        Queue<int> queue = new Queue<int>();

        queue.Enqueue(bobNode);
        bool[] visited = new bool[Input.nbNode];
        
        while(true) 
        {
            int node = queue.Dequeue();
            visited[node] = true;

            foreach(int neighbor in graph[node])
            {
                queue.Enqueue(neighbor);

                if(exits.Contains(neighbor)){
                    int exit = neighbor;
                    return new GameOutput(node, exit);
                }
            }  
        }          
    }
}

class GameLoop
{
    private SmartSearch smart;
    private RandomSearch random;
    public GameOutput Output {get; private set;} 

    public GameLoop(CollectionHandler collection)
    {
        smart = new SmartSearch(collection);
        random = new RandomSearch(collection);
    }

    public void FindDangerousLink(int bobPos)
    {  
        Output = smart.SearchSpecialNode(bobPos);
     
        if(Output == null){
            Output = random.BFS(bobPos);
        }   
    }
} 

class GameOutput
{
    public int Node {get;}
    public int Exit {get;}
    public bool IsCrit {get;}

    public GameOutput(int node, int exit, bool isCrit = false)
    {
        Node = node;
        Exit = exit;
        IsCrit = isCrit;
    }

    public override string ToString()
    {
        return $"{Node} {Exit}";
    }
}
  
class Program
{
    public void Run()
    {
        Network network = new Network();
        network.Build();

        Exit exit = new Exit();
        exit.EnlistExits();

        Crit crit = new Crit(network, exit);
        crit.FindCritNode();
    
        CollectionHandler collection = new CollectionHandler(network, exit, crit);
        GameLoop gameLoop = new GameLoop(collection);
  
        while (true)
        {
            int bobNode = int.Parse(Console.ReadLine());
            gameLoop.FindDangerousLink(bobNode);

            GameOutput output = gameLoop.Output;
            collection.RemoveLink(output); 

            Console.WriteLine(output);      
        }
    }

    public static void Main()
    {
       Program program = new Program();
       program.Run();
    }
}