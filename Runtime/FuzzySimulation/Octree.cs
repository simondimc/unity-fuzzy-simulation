using System.Collections.Generic;
using UnityEngine;

class Octree {

    private int maxDepth;
    private int bucketSize;
    private float xmin;
    private float xmax;
    private float ymin;
    private float ymax;
    private float zmin;
    private float zmax;
    public Vector3 Corner1 { get; private set; }
    public Vector3 Corner2 { get; private set; }

    private List<Agent> agents;
    private Octree nnw;
    private Octree nne;
    private Octree nsw;
    private Octree nse;
    private Octree snw;
    private Octree sne;
    private Octree ssw;
    private Octree sse;

    public Octree(int maxDepth, int bucketSize, float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) {
        this.maxDepth = maxDepth;
        this.bucketSize = bucketSize;
        this.xmin = xmin;
        this.xmax = xmax;
        this.ymin = ymin;
        this.ymax = ymax;
        this.zmin = zmin;
        this.zmax = zmax;
        this.agents = new List<Agent>();
        this.Corner1 = new Vector3(xmin, ymin, zmin);
        this.Corner2 = new Vector3(xmax, ymax, zmax);
    }

    public List<Agent> GetNeighbors(Agent agent, bool directToNode) {
        List<Agent> neighbors = new List<Agent>();
        this.InternalGetNeighbors(agent, directToNode, neighbors);
        return neighbors;
    }

    private void InternalGetNeighbors(Agent agent, bool directToNode, List<Agent> neighbors) {

        Vector3 f = agent.Direction.normalized;
        Vector3 l = Vector3.Cross(f, Vector3.up).normalized;
        Vector3 u = Vector3.Cross(l, f).normalized;

        if (directToNode) {

            if (this.maxDepth <= 0 || this.agents.Count > 0) {
                foreach (Agent agentInNode in this.agents) {
                    if (Utils.AgentInAgentFieldOfView(agent, agentInNode, f, l, u)) {
                        neighbors.Add(agentInNode);
                    }
                }

                return;
            }

            Vector3 position = agent.Position;
            float xmiddle = (xmax - xmin) / 2 + xmin;
            float ymiddle = (ymax - ymin) / 2 + ymin;
            float zmiddle = (zmax - zmin) / 2 + zmin;

            if (position.x > xmiddle) {
                if (position.y > ymiddle) {
                    if (position.z > zmiddle) {
                        if (this.nne != null) {
                            this.nne.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    } else {
                        if (this.nnw != null) {
                            this.nnw.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    }
                } else {
                    if (position.z > zmiddle) {
                        if (this.nse != null) {
                            this.nse.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    } else {
                        if (this.nsw != null) {
                            this.nsw.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    }
                }
            } else {
                if (position.y > ymiddle) {
                    if (position.z > zmiddle) {
                        if (this.sne != null) {
                            this.sne.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    } else {
                        if (this.snw != null) {
                            this.snw.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    }
                } else {
                    if (position.z > zmiddle) {
                        if (this.sse != null) {
                            this.sse.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    } else {
                        if (this.ssw != null) {
                            this.ssw.InternalGetNeighbors(agent, directToNode, neighbors);
                        }
                    }
                }
            }
        } else {

            foreach (Agent agentInNode in this.agents) {
                if (Utils.AgentInAgentFieldOfView(agent, agentInNode, f, l, u)) {
                    neighbors.Add(agentInNode);
                }
            }

            if (this.nnw != null && Utils.BoxSphereIntersection(nnw.Corner1, nnw.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.nnw.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.nne != null && Utils.BoxSphereIntersection(nne.Corner1, nne.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.nne.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.nsw != null && Utils.BoxSphereIntersection(nsw.Corner1, nsw.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.nsw.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.nse != null && Utils.BoxSphereIntersection(nse.Corner1, nse.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.nse.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.snw != null && Utils.BoxSphereIntersection(snw.Corner1, snw.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.snw.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.sne != null && Utils.BoxSphereIntersection(sne.Corner1, sne.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.sne.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.ssw != null && Utils.BoxSphereIntersection(ssw.Corner1, ssw.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.ssw.InternalGetNeighbors(agent, directToNode, neighbors);
            }
            if (this.sse != null && Utils.BoxSphereIntersection(sse.Corner1, sse.Corner2, agent.Position, agent.PerceptionRadius)) {
                this.sse.InternalGetNeighbors(agent, directToNode, neighbors);
            }
        }

    }

    public void Insert(Agent agent) {
        if (this.maxDepth <= 0) {
            this.agents.Add(agent);
            return;
        }

        List<Agent> toAdd = new List<Agent>();

        if (this.agents.Count < this.bucketSize && 
            this.nnw == null && 
            this.nne == null && 
            this.nsw == null && 
            this.nse == null && 
            this.snw == null && 
            this.sne == null && 
            this.ssw == null && 
            this.sse == null) {
            this.agents.Add(agent);
            return;
        } else {
            foreach (Agent a in this.agents) {
                toAdd.Add(a);
            }
            this.agents.Clear();
        }

        toAdd.Add(agent);

        foreach (Agent agentToAdd in toAdd) {

            Vector3 position = agentToAdd.Position;
            float xmiddle = (xmax - xmin) / 2 + xmin;
            float ymiddle = (ymax - ymin) / 2 + ymin;
            float zmiddle = (zmax - zmin) / 2 + zmin;

            if (position.x > xmiddle) {
                if (position.y > ymiddle) {
                    if (position.z > zmiddle) {
                        if (this.nne == null) {
                            this.nne = new Octree(this.maxDepth - 1, this.bucketSize, xmiddle, xmax, ymiddle, ymax, zmiddle, zmax);
                        }
                        this.nne.Insert(agentToAdd);
                    } else {
                        if (this.nnw == null) {
                            this.nnw = new Octree(this.maxDepth - 1, this.bucketSize, xmiddle, xmax, ymiddle, ymax, zmin, zmiddle);
                        }
                        this.nnw.Insert(agentToAdd);
                    }
                } else {
                    if (position.z > zmiddle) {
                        if (this.nse == null) {
                            this.nse = new Octree(this.maxDepth - 1, this.bucketSize, xmiddle, xmax, ymin, ymiddle, zmiddle, zmax);
                        }
                        this.nse.Insert(agentToAdd);
                    } else {
                        if (this.nsw == null) {
                            this.nsw = new Octree(this.maxDepth - 1, this.bucketSize, xmiddle, xmax, ymin, ymiddle, zmin, zmiddle);
                        }
                        this.nsw.Insert(agentToAdd);
                    }
                }
            } else {
                if (position.y > ymiddle) {
                    if (position.z > zmiddle) {
                        if (this.sne == null) {
                            this.sne = new Octree(this.maxDepth - 1, this.bucketSize, xmin, xmiddle, ymiddle, ymax, zmiddle, zmax);
                        }
                        this.sne.Insert(agentToAdd);
                    } else {
                        if (this.snw == null) {
                            this.snw = new Octree(this.maxDepth - 1, this.bucketSize, xmin, xmiddle, ymiddle, ymax, zmin, zmiddle);
                        }
                        this.snw.Insert(agentToAdd);
                    }
                } else {
                    if (position.z > zmiddle) {
                        if (this.sse == null) {
                            this.sse = new Octree(this.maxDepth - 1, this.bucketSize, xmin, xmiddle, ymin, ymiddle, zmiddle, zmax);
                        }
                        this.sse.Insert(agentToAdd);
                    } else {
                        if (this.ssw == null) {
                            this.ssw = new Octree(this.maxDepth - 1, this.bucketSize, xmin, xmiddle, ymin, ymiddle, zmin, zmiddle);
                        }
                        this.ssw.Insert(agentToAdd);
                    }
                }
            }
        }
    }

}