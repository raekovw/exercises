using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Node{
    public static int IdCount = 0;
    public int Id {get;}
    public int X {get;}
    public int Y {get;}
    public int Power {get;}
    public int CurPower {get; set;}
    public int NbActiveNeighbor {get; set;}
    public Node Right {get; set;} = null;
    public Node Bot {get; set;} = null;
    public List<Node> RightBot {get; set;} = new List<Node>();
    public List<Node> Neighbors {get;} = new List<Node>();
    public List<int> AvailableBridges {get;} = new List<int>();

    public Node(int x, int y, int power){
        X = x;
        Y = y;
        Power = power;
        Id = IdCount;
        IdCount++;
    }
}

class NN{
    public Node Node {get;}
    public Node Neighbor {get;}

    public NN(Node node, Node neighbor){
        Node = node;
        Neighbor = neighbor;
    }
}

//***********************************************************************************************************
class Program{
    Node[,] grid;
    int[,] filledGrid;
    private Dictionary<int, Node> nodeId = new Dictionary<int, Node>();
    private List<Node> nodes = new List<Node>();
    private List<Node> safeNodes1L = new List<Node>();
    private List<Node> safeNodes2L = new List<Node>();
    private List<(int, int)> bridges = new List<(int, int)>();
    private List<(int, int)> copyBridges = new List<(int, int)>();
    
    static void Main(){
        Program program = new Program();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        program.Run();
        sw.Stop();
        Console.Error.WriteLine(sw.ElapsedMilliseconds+"ms");

    }

    public void Run(){
        BuildBoardAndData();
        TriggerLogic();
    }

    private void BuildBoardAndData(){
        FillGridWithNode();
        CreateLinkedListForEachNode();
        MatchData();
    }

    private void TriggerLogic(){
        LogicStep();
        PrintBridge();
    }

    private void LogicStep(){
        SafeBridges(2);
        SafeBridges(1);
        if (Finished()) return;

        SecureBridges();
        if (Finished()) return;

        PrintAndLoad(); // first print to avoid timeout
        PurgeList();
        PrintAllComb(); // TO BE CONTINUED
        return;
    }

    //*****************************************BOARD******************************************************************
    public void FillGridWithNode(){
        int width = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());

        grid = new Node[width, height];
        filledGrid = new int[width, height];

        for(int i = 0; i < height; i++){
            int j = 0;
            string line = Console.ReadLine();
            foreach (char ch in line){
                if(ch != '.'){
                    grid[j, i] = new Node(j, i, Convert.ToInt32(ch) - '0');
                }
                j++;
            }
        }
    }

    public void CreateLinkedListForEachNode(){
        for(int i = 0; i < grid.GetLength(0); i++){
            for(int j = 0; j < grid.GetLength(1); j++){
                Node node = grid[i,j];

                if(node != null){
                    FoundClosestRightNode(i, j, node);
                    FoundClosestBotNode(i, j, node);
                    nodes.Add(node);
                }
            }
        }
    }

    private void FoundClosestRightNode(int i, int startNodeJ, Node node){
        for(int j = startNodeJ + 1; j < grid.GetLength(1); j++){
            Node rightNode = grid[i,j];

            if(rightNode != null){
                rightNode.Neighbors.Add(node);
                node.Neighbors.Add(rightNode);
                node.Right = rightNode;
                return;
            }
        }
    }

    private void FoundClosestBotNode(int startNodeI, int j, Node node){
        for(int i = startNodeI + 1; i < grid.GetLength(0); i++){
            Node botNode = grid[i,j];

            if(botNode != null){
                botNode.Neighbors.Add(node);
                node.Neighbors.Add(botNode);
                node.Bot = botNode;
                return;
            }
        }
    }

    public void MatchData(){
        foreach(Node node in nodes){
            HandleSafeNode(node);
            nodeId[node.Id] = node;
            node.CurPower = node.Power;
            node.RightBot.Add(node.Right);
            node.RightBot.Add(node.Bot);
        }
    }

    private void HandleSafeNode(Node node){
        int x = (node.Power % 2) == 0 ? 0 : 1;
        int safetyCriterion = 2 * node.Neighbors.Count - x;

        if(node.Power == safetyCriterion){
            if(x == 0){
                safeNodes2L.Add(node);
            }
            else if(x == 1){
                safeNodes1L.Add(node);
            }
        }
    }

    //**********************************LOGIC*************************************************************************
    private void SafeBridges(int choosenList){
        List<Node> safeNodes = new List<Node>();
        switch(choosenList){
            case 2:
                safeNodes = safeNodes2L;
            break;

            case 1:
                safeNodes = safeNodes1L;
            break;
        }

        foreach(Node node in safeNodes){
            foreach(Node neighbor in node.Neighbors){
                NN duo = new NN(node, neighbor);

                if(CanConnect(duo)){
                    if(ReferenceEquals(safeNodes, safeNodes2L)){
                        AddBridge(duo);
                        AddBridge(duo);
                    }

                    else if(ReferenceEquals(safeNodes, safeNodes1L) && BridgeCount(duo) == 0){
                        AddBridge(duo);
                    }
                }
            }
        }
    }

    private List<Node> GetSecure(){
        return nodes.Where(
            node => node.CurPower > 0 &&
            (node.CurPower == node.AvailableBridges.Sum() ||
            node.CurPower == 1 && node.NbActiveNeighbor == 1)
        ).ToList();
    }

    private void SecureBridges(){
        List<Node> lonelyNodes = GetSecure();
        while(lonelyNodes.Count > 0){

            foreach(Node node in lonelyNodes){
                foreach(Node neighbor in node.Neighbors){
                    NN duo = new NN(node, neighbor);

                    if(CanConnect(duo)){
                        if(node.CurPower % 2 == 0 && BridgeCount(duo) == 0){
                            AddBridge(duo);
                            AddBridge(duo);
                        }
                        else{
                            AddBridge(duo);
                        }
                    }
                }
            }
            lonelyNodes = GetSecure();
        }
    }

//----------------------------------------------------------------------------
    public void PrintAndLoad(){
        copyBridges = bridges.ToList();
        const int bridgeWitdh = 1;
        foreach ((int, int) node in copyBridges){
            Node node1 = nodeId[node.Item1];
            Node node2 = nodeId[node.Item2];
            string bridge = String.Join(" ", node1.X, node1.Y, node2.X, node2.Y, bridgeWitdh);
            Console.WriteLine(bridge);
        }
    }

    public void PrintBridge(){
        const int bridgeWitdh = 1;
        foreach((int, int) node in copyBridges){
            if(bridges.Contains(node)){
                bridges.Remove(node);
            }
        }

        foreach((int, int) node in bridges){
            Node node1 = nodeId[node.Item1];
            Node node2 = nodeId[node.Item2];
            string bridge = String.Join(" ", node1.X, node1.Y, node2.X, node2.Y, bridgeWitdh);
            Console.WriteLine(bridge);
        }
    }

    List<Node> list;
    public void PurgeList(){
        list = new List<Node>(nodes.Where(node => node.CurPower > 0));
    }
 
    //***********************************************************************************************************
    private bool NoPower(Node node){
        return node.CurPower == 0;
    }

    public bool Finished(){
        return nodes.All(node => node.CurPower == 0);
    }

    public void UpdateBoard(){
        CurPowerAndActiveCount();
        AvailableBridgesCount();
    }

    public void CurPowerAndActiveCount(){
        foreach(Node node in nodes){
            node.NbActiveNeighbor = node.Neighbors.Count;
            node.CurPower = node.Power;

            foreach(Node neighbor in node.Neighbors){
                NN duo = new NN(node, neighbor);
                if(Intersect(duo) || BridgeCount(duo) == 2 || NoPower(neighbor)){
                    node.NbActiveNeighbor--;
                }
                node.CurPower-=BridgeCount(duo);
            }
        }
    }

    public void AvailableBridgesCount(){
        foreach(Node node in nodes){
            node.AvailableBridges.Clear();

            foreach(Node neighbor in node.Neighbors){
                NN duo = new NN(node, neighbor);
                int g = ( node.Power == 1 || neighbor.Power == 1) ? 1 : 2;
                if(!Intersect(duo) && !(BridgeCount(duo) == 2) && !NoPower(neighbor)){
                    node.AvailableBridges.Add(g - BridgeCount(duo));
                }
            }
        }
    }

    public bool HasPowerDeficit(){
        return nodes.Any(node => node.CurPower > node.AvailableBridges.Sum());
    }

    public bool CanConnect(NN duo){
        if(BridgeCount(duo) == 2 || Intersect(duo) || NoPower(duo.Neighbor) || NoPower(duo.Node)){
            return false;
        }
        return true;
    }

    public void AddBridge(NN duo){
        bridges.Add(MinMaxPair(duo.Node.Id, duo.Neighbor.Id));
        MapGrid(duo);
        UpdateBoard();
    }

    public (int, int) RemoveBridge(){
        (int, int) pair = bridges[^1];
        NN duo = new NN(nodeId[pair.Item1], nodeId[pair.Item2]);
        UnMapGrid(duo);
        bridges.Remove(pair);
        UpdateBoard();
        return pair;
    }

    public int BridgeCount(NN duo){
        return bridges.Count(pair => pair.Equals(MinMaxPair(duo.Node.Id, duo.Neighbor.Id)));
    }

//****************************MATHS*******************************************************************************
    private (int, int) MinMaxPair(int nodeId, int neighborId){
        return (( Math.Min(nodeId, neighborId), Math.Max(nodeId, neighborId) ));
    }

    private void GetMinMaxValues(int[] array, out int minX, out int maxX, out int minY, out int maxY){
        int x1 = array[0]; int x2 = array[2];
        int y1 = array[1]; int y2 = array[3];

        minX = Math.Min(x1, x2); maxX = Math.Max(x1, x2);
        minY = Math.Min(y1, y2); maxY = Math.Max(y1, y2);
    }
    //******************************INTERSECT*****************************************************************************
    private int[] GetCoords(NN duo){
        return new int[]{duo.Node.X, duo.Node.Y, duo.Neighbor.X, duo.Neighbor.Y};
    }

    private void MapGrid(NN duo){
        int minX, maxX, minY, maxY;
        int[] array = GetCoords(duo);

        GetMinMaxValues(array, out minX, out maxX, out minY, out maxY);
        if(minX == maxX){
            for(int i = minY + 1; i < maxY; i++){
                int constX = minX = maxX;
                filledGrid[constX, i]++;
            }
        }

        else if(minY == maxY){
            for(int i = minX + 1; i < maxX; i++){
                int constY = minY = maxY;
                filledGrid[i, constY]++;
            }
        }
    }

    private void UnMapGrid(NN duo){
        int minX, maxX, minY, maxY;
        int[] array = GetCoords(duo);

        GetMinMaxValues(array, out minX, out maxX, out minY, out maxY);
        if(minX == maxX){
            int constX = minX = maxX;
            for(int i = minY + 1; i < maxY; i++){
                filledGrid[constX, i]--;
            }
        }

        else if(minY == maxY){
            int constY = minY = maxY;
            for(int i = minX + 1; i < maxX; i++){
                filledGrid[i, constY]--;
            }
        }
    }

    private bool Intersect(NN duo){
        if(BridgeCount(duo) > 0){
            return false;
        }

        int minX, maxX, minY, maxY;
        int[] array = GetCoords(duo);
        (int, int) pair = MinMaxPair(duo.Node.Id, duo.Neighbor.Id);
        GetMinMaxValues(array, out minX, out maxX, out minY, out maxY);

        if(minX == maxX){
            int constX = minX = maxX;
            for(int y = minY + 1; y < maxY; y++){
                if(filledGrid[constX, y] > 0){
                    return true;
                }
            }
        }

        else if(minY == maxY){
            int constY = minY = maxY;
            for(int x = minX + 1; x < maxX; x++){
                if(filledGrid[x, constY] > 0){
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsIsolate(Node n){
        Stack<Node> stack = new();
        bool[] visited = new bool[Node.IdCount];
        bool hasNeigh = false;
        stack.Push(n);

        while(stack.Count > 0){
            Node node = stack.Peek();
            visited[node.Id] = true;

            if(node.CurPower != 0) return false;
            visited[node.Id] = true;

            foreach(Node neighbor in node.Neighbors){
                NN duo = new NN(node, neighbor);

                if(!visited[neighbor.Id]){
                    visited[neighbor.Id] = true;
                    hasNeigh = true;

                    if(BridgeCount(duo) == 0 || Intersect(duo)) continue;
                    if(neighbor.CurPower != 0) return false;

                    stack.Push(neighbor);
                    break;
                }
            }
            if(!hasNeigh) stack.Pop();
        }
        return true;
    }

//***********************************************************************************************************
    private void PrintAllComb(){
        Dictionary<int, List<List<(int, int)>>> allComb = GetEachBridge();
        foreach(var nodeList in allComb){
            foreach(List<(int, int)> lol in nodeList.Value){
                foreach((int, int) pair in lol){
                    Console.Error.WriteLine($"{pair.Item1}, {pair.Item2}");
                }
                Console.Error.WriteLine();
            }
        }
    }

    private Dictionary<int, List<List<(int, int)>>> GetEachBridge(){
        List<Node> poweredNode = list.ToList();
        Dictionary<int, List<List<(int, int)>>> allComb = new();
        foreach(Node node in poweredNode){       
            List<List<(int, int)>> bridges = GenerateNodeComb(node);
            allComb[node.Id] = bridges;
        }
        return allComb;
    }

   
    private List<List<(int, int)>> GenerateNodeComb(Node node){
        string[] comb = CombPerPower(node.Power);
        List<List<(int, int)>> bridgesNode = new();
        foreach(string s in comb){
            int countI = s.Count(c => c == 'i');
            int countJ = s.Count(c => c == 'j');
            bridgesNode.Add(BuildBridgeRB(node, countI, countJ));
        }
        return bridgesNode;
    }

    public List<(int, int)> BuildBridgeRB(Node node, int nbI, int nbJ){
        List<(int, int)> bridgesRB = new List<(int, int)>();
        for(int i = 0; i < nbI; i++){
            if(node.RightBot[0] == null) continue;
            bridgesRB.Add(MinMaxPair(node.Id, node.RightBot[0].Id));
        }
        for(int j = 0; j < nbJ; j++){
            if(node.RightBot[1] == null) continue;
            bridgesRB.Add(MinMaxPair(node.Id, node.RightBot[1].Id));
        }
        return bridgesRB;
    }

  
    public string[] CombPerPower(int power){
       List<string> allComb = new List<string>{"i", "j", "ii", "jj", "ij", "ijj", "jii", "iijj"};
    //    List<string> comb2 = new List<string>{"0", "i", "ii", "ij", "ijj", "iijj"};
       string[] comb;
        switch(power){
            case 1:
                comb = allComb.Take(2).ToArray();
            break;
            case 2:
                comb = allComb.Take(5).ToArray();
            break;
            case 3:
                comb = allComb.Take(7).ToArray();
            break;
            default:
                comb = allComb.ToArray();
            break;
        }
        return comb;
    }
}
