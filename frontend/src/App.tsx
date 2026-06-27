import './App.css';

import { useEffect, useState } from 'react';

import type { TopologyGraph as TopologyGraphData } from './models/graph';
import { TopologyGraph } from './components/TopologyGraph';

const apiBaseUrl = import.meta.env.VITE_TOPOLOGY_API_BASE_URL ?? '';

function App() {
  const [graph, setGraph] = useState<TopologyGraphData | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const abortController = new AbortController();

    async function loadTopology() {
      try {
        const response = await fetch(`${apiBaseUrl}/api/topology`, {
          signal: abortController.signal,
        });

        if (!response.ok) {
          throw new Error(`Topology API returned ${response.status}`);
        }

        const topology = await response.json() as TopologyGraphData;
        setGraph(topology);
        setError(null);
      } catch (caught) {
        if (caught instanceof DOMException && caught.name === 'AbortError') {
          return;
        }

        setError(caught instanceof Error ? caught.message : 'Failed to load topology');
      }
    }

    void loadTopology();

    return () => abortController.abort();
  }, []);

  if (error) {
    return (
      <main className="app-status" role="alert">
        <h1>Topology unavailable</h1>
        <p>{error}</p>
      </main>
    );
  }

  if (!graph) {
    return (
      <main className="app-status" aria-busy="true">
        <p>Loading topology...</p>
      </main>
    );
  }

  return <TopologyGraph graph={graph} />;
}

export default App;
