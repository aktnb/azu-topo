import type { Meta, StoryObj } from '@storybook/react-vite';
import { expect, waitFor } from 'storybook/test';

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

const branchGraph: TopologyGraphData = {
  nodes: [
    {
      id: 'fn-broadcast',
      type: 'function',
      name: 'BroadcastEvent',
      property: { functionAppName: 'event-processor', enabled: true },
    },
    {
      id: 'sbq-email',
      type: 'serviceBusQueue',
      name: 'email-queue',
      property: { namespace: 'my-servicebus', status: 'Active' },
    },
    {
      id: 'sbq-sms',
      type: 'serviceBusQueue',
      name: 'sms-queue',
      property: { namespace: 'my-servicebus', status: 'Active' },
    },
    {
      id: 'fn-email',
      type: 'function',
      name: 'SendEmail',
      property: { functionAppName: 'notification-app', enabled: true },
    },
    {
      id: 'fn-sms',
      type: 'function',
      name: 'SendSms',
      property: { functionAppName: 'notification-app', enabled: true },
    },
  ],
  edges: [
    { id: 'e1', source: 'fn-broadcast', target: 'sbq-email', type: 'output' },
    { id: 'e2', source: 'fn-broadcast', target: 'sbq-sms', type: 'output' },
    { id: 'e3', source: 'sbq-email', target: 'fn-email', type: 'trigger' },
    { id: 'e4', source: 'sbq-sms', target: 'fn-sms', type: 'trigger' },
  ],
  warnings: [],
};

const mergeGraph: TopologyGraphData = {
  nodes: [
    {
      id: 'fn-payment',
      type: 'function',
      name: 'ProcessPayment',
      property: { functionAppName: 'payment-app', enabled: true },
    },
    {
      id: 'fn-refund',
      type: 'function',
      name: 'ProcessRefund',
      property: { functionAppName: 'payment-app', enabled: true },
    },
    {
      id: 'sbq-audit',
      type: 'serviceBusQueue',
      name: 'audit-queue',
      property: { namespace: 'my-servicebus', status: 'Active' },
    },
    {
      id: 'fn-audit',
      type: 'function',
      name: 'WriteAuditLog',
      property: { functionAppName: 'audit-app', enabled: true },
    },
  ],
  edges: [
    { id: 'e1', source: 'fn-payment', target: 'sbq-audit', type: 'output' },
    { id: 'e2', source: 'fn-refund', target: 'sbq-audit', type: 'output' },
    { id: 'e3', source: 'sbq-audit', target: 'fn-audit', type: 'trigger' },
  ],
  warnings: [],
};

export const Default: Story = {
  play: async ({ canvas }) => {
    await waitFor(() => expect(canvas.getByText('ProcessOrder')).toBeVisible());
    await expect(canvas.getByText('HandleOrder')).toBeVisible();
    await expect(canvas.getByText('orders-queue')).toBeVisible();
    await expect(canvas.getAllByText('order-processor / Function', { exact: false })).toHaveLength(2);
    await expect(canvas.getByText('my-servicebus / Service Bus Queue', { exact: false })).toBeVisible();
  },
};

export const Branching: Story = {
  args: { graph: branchGraph },
  play: async ({ canvas }) => {
    await waitFor(() => expect(canvas.getByText('BroadcastEvent')).toBeVisible());
    await expect(canvas.getByText('email-queue')).toBeVisible();
    await expect(canvas.getByText('sms-queue')).toBeVisible();
    await expect(canvas.getByText('SendEmail')).toBeVisible();
    await expect(canvas.getByText('SendSms')).toBeVisible();
  },
};

export const Merging: Story = {
  args: { graph: mergeGraph },
  play: async ({ canvas }) => {
    await waitFor(() => expect(canvas.getByText('ProcessPayment')).toBeVisible());
    await expect(canvas.getByText('ProcessRefund')).toBeVisible();
    await expect(canvas.getByText('audit-queue')).toBeVisible();
    await expect(canvas.getByText('WriteAuditLog')).toBeVisible();
  },
};
