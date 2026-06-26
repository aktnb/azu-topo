import type { Meta, StoryObj } from '@storybook/react-vite';
import { expect } from 'storybook/test';

import type { TopologyGraph as TopologyGraphData } from '../../models/graph';
import { TopologyGraph } from './TopologyGraph';

const sampleGraph: TopologyGraphData = {
  nodes: [
    {
      id: 'fn-1',
      type: 'function',
      name: 'ProcessOrder',
      property: { functionAppName: 'order-processor', enabled: true },
    },
    {
      id: 'sbq-1',
      type: 'serviceBusQueue',
      name: 'orders-queue',
      property: { namespace: 'my-servicebus', status: 'Active' },
    },
    {
      id: 'fn-2',
      type: 'function',
      name: 'HandleOrder',
      property: { functionAppName: 'order-processor', enabled: true },
    },
  ],
  edges: [
    { id: 'fn-1-sbq-1', source: 'fn-1', target: 'sbq-1', type: 'output' },
    { id: 'sbq-1-fn-2', source: 'sbq-1', target: 'fn-2', type: 'trigger' },
  ],
  warnings: [],
};

const meta = {
  component: TopologyGraph,
  tags: ['ai-generated'],
  args: {
    graph: sampleGraph,
  },
  decorators: [
    (Story: React.ComponentType) => (
      <div style={{ width: '900px', height: '500px' }}>
        <Story />
      </div>
    ),
  ],
} satisfies Meta<typeof TopologyGraph>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {
  play: async ({ canvas }) => {
    await expect(canvas.getByText('ProcessOrder')).toBeVisible();
    await expect(canvas.getByText('HandleOrder')).toBeVisible();
    await expect(canvas.getByText('orders-queue')).toBeVisible();
    await expect(canvas.getByText('order-processor / Function', { exact: false })).toBeVisible();
    await expect(canvas.getByText('my-servicebus / Service Bus Queue', { exact: false })).toBeVisible();
  },
};
