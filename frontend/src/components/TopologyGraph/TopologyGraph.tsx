import { ReactFlow, Background, Controls, Handle, Position, MarkerType } from '@xyflow/react';
import type { Node, NodeProps } from '@xyflow/react';
import '@xyflow/react/dist/style.css';

import type { TopologyGraph as TopologyGraphData } from '../../models/graph';
import type { FunctionNodeProperties, ServiceBusQueueNodeProperties, TopologyNodeType } from '../../models/node';
import { FunctionNode } from '../nodes/FunctionNode';
import { ServiceBusQueueNode } from '../nodes/ServiceBusQueueNode';
import styles from './TopologyGraph.module.scss';
import { toFlowEdges, toFlowNodes } from './toFlowGraph';

type FunctionNodeType = Node<{ name: string } & FunctionNodeProperties, 'function'>;
type ServiceBusQueueNodeType = Node<{ name: string } & ServiceBusQueueNodeProperties, 'serviceBusQueue'>;

function FunctionNodeWrapper({ data }: NodeProps<FunctionNodeType>) {
  return (
    <>
      <Handle type="target" position={Position.Left} />
      <FunctionNode {...data} />
      <Handle type="source" position={Position.Right} />
    </>
  );
}

function ServiceBusQueueNodeWrapper({ data }: NodeProps<ServiceBusQueueNodeType>) {
  return (
    <>
      <Handle type="target" position={Position.Left} />
      <ServiceBusQueueNode {...data} />
      <Handle type="source" position={Position.Right} />
    </>
  );
}

function UnimplementedNodeWrapper({ data }: NodeProps) {
  return <div>{String(data.name)} (unimplemented)</div>;
}

const nodeTypes = {
  function: FunctionNodeWrapper,
  serviceBusQueue: ServiceBusQueueNodeWrapper,
  serviceBusTopic: UnimplementedNodeWrapper,
  serviceBusSubscription: UnimplementedNodeWrapper,
} satisfies Record<TopologyNodeType, unknown>;

const defaultEdgeOptions = {
  style: { stroke: '#8b949e', strokeWidth: 1.5 },
  markerEnd: { type: MarkerType.ArrowClosed, color: '#8b949e' },
} as const;

type Props = {
  graph: TopologyGraphData;
};

export function TopologyGraph({ graph }: Props) {
  const nodes = toFlowNodes(graph.nodes, graph.edges);
  const edges = toFlowEdges(graph.edges);

  return (
    <div className={styles.root}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        defaultEdgeOptions={defaultEdgeOptions}
        fitView
      >
        <Background />
        <Controls />
      </ReactFlow>
    </div>
  );
}
