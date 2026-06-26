import './App.css';

import type { TopologyGraph as TopologyGraphData } from './models/graph';
import { TopologyGraph } from './components/TopologyGraph';

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

function App() {
  return <TopologyGraph graph={sampleGraph} />;
}

export default App;
