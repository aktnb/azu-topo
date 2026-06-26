import type { Edge, Node } from '@xyflow/react';

import type { TopologyEdge } from '../../models/edge';
import type { TopologyNode } from '../../models/node';

export function toFlowNodes(nodes: TopologyNode[]): Node[] {
  return nodes.map(({ id, type, name, property }, index) => ({
    id,
    type,
    position: { x: index * 320, y: 0 },
    data: { name, ...property },
  }));
}

export function toFlowEdges(edges: TopologyEdge[]): Edge[] {
  return edges.map(({ id, source, target, type }) => ({
    id,
    source,
    target,
    data: { edgeType: type },
  }));
}
