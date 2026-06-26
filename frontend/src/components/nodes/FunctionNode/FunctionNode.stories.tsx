import type { Meta, StoryObj } from "@storybook/react-vite";
import { expect } from "storybook/test";
import { FunctionNode } from "./FunctionNode";

const meta = {
  component: FunctionNode,
  tags: ["ai-generated"],
  args: {
    name: "ProcessOrder",
    enabled: true,
  },
} satisfies Meta<typeof FunctionNode>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Enabled: Story = {
  play: async ({ canvas }) => {
    await expect(canvas.getByText("ProcessOrder")).toBeVisible();
    await expect(canvas.getByText("Enabled")).toBeVisible();
  },
};

export const Disabled: Story = {
  args: {
    enabled: false,
  },
};

export const LongName: Story = {
  args: {
    name: "VeryLongFunctionNameThatShouldBeClipped",
  },
};

// Verifies that the SCSS module loaded: .root has background: white
export const CssCheck: Story = {
  play: async ({ canvas }) => {
    // "Function App" subtext is a direct child of .root — its parentElement is the root card
    const root = canvas.getByText("Function App").parentElement!;
    await expect(getComputedStyle(root).backgroundColor).toBe(
      "rgb(255, 255, 255)"
    );
  },
};
