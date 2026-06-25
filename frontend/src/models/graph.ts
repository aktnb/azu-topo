import type { TopologyEdge } from './edge';
import type { TopologyMetric } from './metrics';
import type { TopologyNode } from './node';
import type { TopologyWarning } from './warning';

export type TopologyGraph = {
  nodes: TopologyNode[];
  edges: TopologyEdge[];
  metrics?: TopologyMetric[];
  warnings: TopologyWarning[];
};
