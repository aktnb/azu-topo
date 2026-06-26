import { Graph, layout } from '@dagrejs/dagre';
import type { Edge, Node } from '@xyflow/react';

import type { TopologyEdge } from '../../models/edge';
import type { TopologyNode } from '../../models/node';

// Must match .root { width } in FunctionNode.module.scss / ServiceBusQueueNode.module.scss
const NODE_WIDTH = 220;
const NODE_HEIGHT = 80;

export function toFlowNodes(nodes: TopologyNode[], edges: TopologyEdge[]): Node[] {
  const graph = new Graph();
  graph.setGraph({ rankdir: 'LR', nodesep: 40, ranksep: 100 });
  // dagre requires a default edge label; omitting this causes runtime errors during layout
  graph.setDefaultEdgeLabel(() => ({}));

  for (const { id } of nodes) {
    graph.setNode(id, { width: NODE_WIDTH, height: NODE_HEIGHT });
  }
  for (const { source, target } of edges) {
    graph.setEdge(source, target);
  }

  layout(graph);

  return nodes.map(({ id, type, name, property }) => {
    const pos = graph.node(id);
    if (!pos) throw new Error(`dagre layout: node "${id}" has no position`);
    return {
      id,
      type,
      position: { x: pos.x - NODE_WIDTH / 2, y: pos.y - NODE_HEIGHT / 2 },
      data: { name, ...property },
    };
  });
}

export function toFlowEdges(edges: TopologyEdge[]): Edge[] {
  return edges.map(({ id, source, target, type }) => ({
    id,
    source,
    target,
    data: { edgeType: type },
  }));
}
