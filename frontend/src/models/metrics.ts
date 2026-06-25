export type TopologyMetric = {
  nodeId: string;
  values: MetricValue[];
};

export type MetricValue = {
  name: string;
  value: number | null;
  unit?: string;
};
