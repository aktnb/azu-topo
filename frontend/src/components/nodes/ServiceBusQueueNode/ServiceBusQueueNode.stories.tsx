import type { Meta, StoryObj } from '@storybook/react-vite';
import { expect } from 'storybook/test';
import { ServiceBusQueueNode } from './ServiceBusQueueNode';

const meta = {
  component: ServiceBusQueueNode,
  tags: ['ai-generated'],
  args: {
    name: 'orders-queue',
    namespace: 'my-servicebus',
    status: 'Active',
  },
} satisfies Meta<typeof ServiceBusQueueNode>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Active: Story = {
  play: async ({ canvas }) => {
    await expect(canvas.getByText('orders-queue')).toBeVisible();
    await expect(canvas.getByText('Active')).toBeVisible();
  },
};

export const Disabled: Story = {
  args: {
    status: 'Disabled',
  },
};

export const SendDisabled: Story = {
  args: {
    status: 'SendDisabled',
  },
};

export const ReceiveDisabled: Story = {
  args: {
    status: 'ReceiveDisabled',
  },
};

export const LongName: Story = {
  args: {
    name: 'very-long-queue-name-that-should-be-clipped',
  },
};

export const CssCheck: Story = {
  play: async ({ canvas }) => {
    const root = canvas.getByText('Service Bus Queue', { exact: false }).parentElement!;
    await expect(getComputedStyle(root).backgroundColor).toBe('rgb(255, 255, 255)');
  },
};
