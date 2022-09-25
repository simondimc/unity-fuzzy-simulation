using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class FuzzyRules : EditorWindow {

    public static FuzzyController FuzzyController;
    public static Drive Drive;

    private List<EditorElement> editorElements;
    private Vector2 offset;
    private Vector2 drag;
    private EEConnectionToMake connectionToMake;

    private EESidePanel sidePanel;
    private EEDriveInfo driveInfo;
    private EEConnectedComponents connectedComponents;
    private EEZoomPanel zoomPanel;
    private Vector2[] locations;

    private GUIStyle backgroundStyle;

    private static void OpenWindow() {
        FuzzyRules window = GetWindow<FuzzyRules>();
        window.titleContent = new GUIContent("Fuzzy Rules");
    }

    public static void Open(FuzzyController fuzzyController) {
        FuzzyRules.FuzzyController = fuzzyController;
        FuzzyRules.OpenWindow();
    }

    public void OpenDrive(Drive drive) {
        this.driveInfo.SetDrive(drive);

        drive.isOpen = true;
        FuzzyRules.FuzzyController.SetDrive(drive.guid, drive);
        FuzzyRules.Drive = drive;

        if (FuzzyRules.FuzzyController == null) return;

        this.editorElements.Clear();

        foreach (GraphNode graphNode in FuzzyRules.FuzzyController.GetInputGraphNodes(FuzzyRules.Drive)) {
            this.editorElements.Add(new EEGraphNode(graphNode, EEGraphNodeType.Input, AddConnection, DeleteNode, NodeClicked));
        }

        foreach (GraphNode graphNode in FuzzyRules.FuzzyController.GetOutputGraphNodes(FuzzyRules.Drive)) {
            this.editorElements.Add(new EEGraphNode(graphNode, EEGraphNodeType.Output, AddConnection, DeleteNode, NodeClicked));
        }

        foreach (AndNode andNode in FuzzyRules.FuzzyController.GetAndNodes(FuzzyRules.Drive)) {
            this.editorElements.Add(new EEAndNode(andNode, AddConnection, DeleteNode, NodeClicked));
        }

        foreach (OrNode orNode in FuzzyRules.FuzzyController.GetOrNodes(FuzzyRules.Drive)) {
            this.editorElements.Add(new EEOrNode(orNode, AddConnection, DeleteNode, NodeClicked));
        }

        foreach (NotNode notNode in FuzzyRules.FuzzyController.GetNotNodes(FuzzyRules.Drive)) {
            this.editorElements.Add(new EENotNode(notNode, AddConnection, DeleteNode, NodeClicked));
        }

        foreach (Connection connection in FuzzyRules.FuzzyController.GetConnections(FuzzyRules.Drive)) {
            this.editorElements.Add(new EEConnection(connection, SetConnectionNodes, DeleteConnection));
        }

        this.CalcConnectedComponents();
        this.connectedComponents.SetComponent(0);
    }

    private void CalcConnectedComponents() {
        Dictionary<string, Node> guidNode = new Dictionary<string, Node>();
        List<Node> nodes = new List<Node>();

        foreach (GraphNode graphNode in FuzzyRules.FuzzyController.GetInputGraphNodes(FuzzyRules.Drive)) {
            guidNode.Add(graphNode.guid, graphNode);
            nodes.Add(graphNode);
        }

        foreach (GraphNode graphNode in FuzzyRules.FuzzyController.GetOutputGraphNodes(FuzzyRules.Drive)) {
            guidNode.Add(graphNode.guid, graphNode);
            nodes.Add(graphNode);
        }

        foreach (AndNode andNode in FuzzyRules.FuzzyController.GetAndNodes(FuzzyRules.Drive)) {
            guidNode.Add(andNode.guid, andNode);
            nodes.Add(andNode);
        }

        foreach (OrNode orNode in FuzzyRules.FuzzyController.GetOrNodes(FuzzyRules.Drive)) {
            guidNode.Add(orNode.guid, orNode);
            nodes.Add(orNode);
        }

        foreach (NotNode notNode in FuzzyRules.FuzzyController.GetNotNodes(FuzzyRules.Drive)) {
            guidNode.Add(notNode.guid, notNode);
            nodes.Add(notNode);
        }

        Dictionary<Node, int> nodeComponent = new Dictionary<Node, int>();
        int componentNumber = 0;

        foreach (Connection connection in FuzzyRules.FuzzyController.GetConnections(FuzzyRules.Drive)) {
            Node node1 = guidNode[connection.node1Guid];
            Node node2 = guidNode[connection.node2Guid];
            if (!nodeComponent.ContainsKey(node1) && !nodeComponent.ContainsKey(node2)) {
                nodeComponent[node1] = componentNumber;
                nodeComponent[node2] = componentNumber;
                componentNumber += 1;
                nodes.Remove(node1);
                nodes.Remove(node2);
            } else if (!nodeComponent.ContainsKey(node1)) {
                nodeComponent[node1] = nodeComponent[node2];
                nodes.Remove(node1);
            } else if (!nodeComponent.ContainsKey(node2)) {
                nodeComponent[node2] = nodeComponent[node1];
                nodes.Remove(node2);
            }
        }

        foreach (Node node in nodes) {
            nodeComponent[node] = componentNumber;
            componentNumber += 1;
        }

        locations = new Vector2[componentNumber];
        int[] counts = new int[componentNumber];

        for (int i = 0; i < componentNumber; i++) {
            locations[i] = new Vector2(0, 0);
            counts[i] = 0;
        }
        
        foreach (Node node in nodeComponent.Keys) {
            int component = nodeComponent[node];
            locations[component].x += node.x;
            locations[component].y += node.y;
            counts[component] += 1;
        }

        for (int i = 0; i < locations.Length; i++) {
            locations[i] /= counts[i];
        }

        this.connectedComponents.SetNumOfComponents(componentNumber);
    }

    private void OnEnable() {
        this.connectionToMake = null;

        this.backgroundStyle = new GUIStyle();
        this.backgroundStyle.normal.background = EEUtils.ColorTexture(EETheme.MainColor);

        this.sidePanel = new EESidePanel(0, 0, 400, this.position.height, this.OpenDrive);
        this.driveInfo = new EEDriveInfo(400 + 10, 10, 200, 30);
        this.connectedComponents = new EEConnectedComponents(400 + 10 + 200 + 10, 10, 180, 30, this.FocusComponent);
        this.zoomPanel = new EEZoomPanel(400 + 10 + 200 + 10 + 180 + 10, 10, 100, 30);

        this.editorElements = new List<EditorElement>();

        IEnumerable<Drive> drives = FuzzyRules.FuzzyController.GetDrives();
  
        Drive openDrive = drives.Where(x => x.isOpen).First();

        foreach (Drive drive in drives.ToList()) {
            drive.isOpen = false;
            FuzzyRules.FuzzyController.SetDrive(drive.guid, drive);
        }

        if (openDrive != null) this.OpenDrive(openDrive);
        else this.OpenDrive(drives.First());
    }

    private void OnGUI() {
        this.LayerSort();
        this.EventOverlap(new EEEvent(Event.current));
        this.Draw();
        this.ProcessEvent(new EEEvent(Event.current));
        if (GUI.changed) Repaint();
    }

    private void LayerSort() {
        this.editorElements = this.editorElements.OrderBy(x => x.Layer()).ToList();
    }

    private void EventOverlap(EEEvent e) {
        this.sidePanel.EventOverlap(e);
        this.driveInfo.EventOverlap(e);
        this.connectedComponents.EventOverlap(e);
        this.zoomPanel.EventOverlap(e);

        for (int i = this.editorElements.Count - 1; i >= 0; i--) {
            this.editorElements[i].EventOverlap(e);
        }
    }

    private void Draw() {
        this.sidePanel.SetHeight(this.position.height);

        GUI.Box(new Rect(0, 0, this.position.width, this.position.height), "", this.backgroundStyle);

        this.DrawGrid(20, 0.2f, EETheme.LightColor);

        for (int i = 0; i < this.editorElements.Count; i++) {
            this.editorElements[i].DrawAfterOverlapCheck();
        }

        this.sidePanel.DrawAfterOverlapCheck();
        this.driveInfo.DrawAfterOverlapCheck();
        this.connectedComponents.DrawAfterOverlapCheck();
        this.zoomPanel.DrawAfterOverlapCheck();
    }

    private void ProcessEvent(EEEvent e) {
        this.drag = Vector2.zero;

        this.sidePanel.ProcessEvent(e);
        this.driveInfo.ProcessEvent(e);
        this.connectedComponents.ProcessEvent(e);
        this.zoomPanel.ProcessEvent(e);

        for (int i = this.editorElements.Count - 1; i >= 0; i--) {
            this.editorElements[i].ProcessEvent(e);
        }

        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0) {
                    if (this.connectionToMake != null) {
                        this.DeleteConnectionToMake();
                    }
                }
                if (e.GetEvent().button == 1) {
                    ProcessContextMenu(e.GetEvent().mousePosition);
                }
                break;
            case EventType.MouseDrag:
                if (e.GetEvent().button == 0) {
                    Move(e.GetEvent().delta);
                }
                break;
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition) {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Create Input Node"), false, () => AddInputGraphNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Create Output Node"), false, () => AddOutputGraphNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Create And Node"), false, () => AddAndNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Create Or Node"), false, () => AddOrNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Create Not Node"), false, () => AddNotNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void AddInputGraphNode(Vector2 mousePosition) {
        this.editorElements.Add(new EEGraphNode(mousePosition, EEGraphNodeType.Input, AddConnection, DeleteNode, NodeClicked));
        this.CalcConnectedComponents();
    }

    private void AddOutputGraphNode(Vector2 mousePosition) {
        this.editorElements.Add(new EEGraphNode(mousePosition, EEGraphNodeType.Output, AddConnection, DeleteNode, NodeClicked));
        this.CalcConnectedComponents();
    }

    private void AddAndNode(Vector2 mousePosition) {
        this.editorElements.Add(new EEAndNode(mousePosition, AddConnection, DeleteNode, NodeClicked));
        this.CalcConnectedComponents();
    }

    private void AddOrNode(Vector2 mousePosition) {
        this.editorElements.Add(new EEOrNode(mousePosition, AddConnection, DeleteNode, NodeClicked));
        this.CalcConnectedComponents();
    }

    private void AddNotNode(Vector2 mousePosition) {
        this.editorElements.Add(new EENotNode(mousePosition, AddConnection, DeleteNode, NodeClicked));
        this.CalcConnectedComponents();
    }

    private void AddConnection(EENode node) {
        this.connectionToMake = new EEConnectionToMake(node);
        this.editorElements.Add(this.connectionToMake);
    }

    private void DeleteNode(EENode node) {
        foreach(EditorElement editorElement in this.editorElements) {
            if (editorElement is EEConnection && (((EEConnection)editorElement).GetNode1().GetNode().guid == node.GetNode().guid || ((EEConnection)editorElement).GetNode2().GetNode().guid == node.GetNode().guid)) {
                DeleteConnection((EEConnection)editorElement);
            }
        }

        if (node is EEGraphNode && ((EEGraphNode)node).GetGraphNodeType() == EEGraphNodeType.Input) {
            EEGraphNode graphNode = (EEGraphNode)node;
            FuzzyRules.FuzzyController.RemoveInputGraphNode(FuzzyRules.Drive, (GraphNode)graphNode.GetNode());
            this.editorElements = this.editorElements.Where(x => !(x is EEGraphNode && ((EEGraphNode)x).GetNode().guid == ((EEGraphNode)node).GetNode().guid)).ToList();
        } else if (node is EEGraphNode && ((EEGraphNode)node).GetGraphNodeType() == EEGraphNodeType.Output) {
            EEGraphNode graphNode = (EEGraphNode)node;
            FuzzyRules.FuzzyController.RemoveOutputGraphNode(FuzzyRules.Drive, (GraphNode)graphNode.GetNode());
            this.editorElements = this.editorElements.Where(x => !(x is EEGraphNode && ((EEGraphNode)x).GetNode().guid == ((EEGraphNode)node).GetNode().guid)).ToList();
        } else if (node is EEAndNode) {
            EEAndNode andNode = (EEAndNode)node;
            FuzzyRules.FuzzyController.RemoveAndNode(FuzzyRules.Drive, (AndNode)andNode.GetNode());
            this.editorElements = this.editorElements.Where(x => !(x is EEAndNode && ((EEAndNode)x).GetNode().guid == ((EEAndNode)node).GetNode().guid)).ToList();
        } else if (node is EEOrNode) {
            EEOrNode orNode = (EEOrNode)node;
            FuzzyRules.FuzzyController.RemoveOrNode(FuzzyRules.Drive, (OrNode)orNode.GetNode());
            this.editorElements = this.editorElements.Where(x => !(x is EEOrNode && ((EEOrNode)x).GetNode().guid == ((EEOrNode)node).GetNode().guid)).ToList();
        } else if (node is EENotNode) {
            EENotNode notNode = (EENotNode)node;
            FuzzyRules.FuzzyController.RemoveNotNode(FuzzyRules.Drive, (NotNode)notNode.GetNode());
            this.editorElements = this.editorElements.Where(x => !(x is EENotNode && ((EENotNode)x).GetNode().guid == ((EENotNode)node).GetNode().guid)).ToList();
        }
        
        this.CalcConnectedComponents();
    }

    private void DeleteConnectionToMake() {
        this.editorElements.Remove(this.connectionToMake);
        this.connectionToMake = null;
        this.CalcConnectedComponents();
    }

    private void DeleteConnection(EEConnection connection) {
        FuzzyRules.FuzzyController.RemoveConnection(FuzzyRules.Drive, connection.GetConnection());
        this.editorElements = this.editorElements.Where(x => !(x is EEConnection && ((EEConnection)x).GetConnection().node1Guid == connection.GetNode1().GetNode().guid && ((EEConnection)x).GetConnection().node2Guid == connection.GetNode2().GetNode().guid)).ToList();
        this.CalcConnectedComponents();
    }

    private void SetConnectionNodes(EEConnection connection) {
        EENode node1 = null;
        EENode node2 = null;

        foreach (EditorElement editorElement in this.editorElements) {
            if (editorElement is EENode && ((EENode)editorElement).GetNode().guid == connection.GetConnection().node1Guid) {
                node1 = (EENode)editorElement;
            }
            if (editorElement is EENode && ((EENode)editorElement).GetNode().guid == connection.GetConnection().node2Guid) {
                node2 = (EENode)editorElement;
            }
        }

        connection.SetNode1(node1);
        connection.SetNode2(node2);
    }

    private void NodeClicked(EENode node) {
        if (this.connectionToMake != null) {
            // output ->
            if (this.connectionToMake.GetNode1() is EEGraphNode && ((EEGraphNode)this.connectionToMake.GetNode1()).GetGraphNodeType() == EEGraphNodeType.Output) {
                this.DeleteConnectionToMake();
            // -> input
            } else if (node is EEGraphNode && ((EEGraphNode)node).GetGraphNodeType() == EEGraphNodeType.Input) {
                this.DeleteConnectionToMake();
            // not note samo en vhod
            } else if (node is EENotNode && this.ConnectionsToNode(node).Count > 0) {
                this.DeleteConnectionToMake();
            } else {
                this.editorElements.Add(new EEConnection(this.connectionToMake.GetNode1(), node, DeleteConnection));
                this.DeleteConnectionToMake();
            }
        }
    }

    private List<EEConnection> ConnectionsToNode(EENode node) {
        List<EEConnection> connections = new List<EEConnection>();
        foreach (EditorElement editorElement in this.editorElements) {
            if (editorElement is EEConnection) {
                EEConnection connection = (EEConnection)editorElement;
                if (connection.GetConnection().node2Guid == node.GetNode().guid) {
                    connections.Add(connection);
                }
            }
        }
        return connections;
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
        int widthDivs = Mathf.CeilToInt(this.position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(this.position.height / gridSpacing);
 
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
 
        offset += this.drag;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
 
        for (int i = 0; i < widthDivs; i++) {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, this.position.height, 0f) + newOffset);
        }
 
        for (int j = 0; j < heightDivs; j++) {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(this.position.width, gridSpacing * j, 0f) + newOffset);
        }
 
        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void Move(Vector2 delta) {
        this.drag = delta;
 
        for (int i = 0; i < this.editorElements.Count; i++) {
            if (this.editorElements[i] is EEFrame) {
                ((EEFrame)this.editorElements[i]).Move(delta);
            }
        }
 
        GUI.changed = true;
    }

    private void FocusComponent(int component) {
        this.CalcConnectedComponents();
        Move(-this.locations[component] + new Vector2(this.position.width * 0.6f, this.position.height / 2));
    }

}
