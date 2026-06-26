import styles from './ServiceBusQueueNode.module.scss';

type QueueStatus = 'Active' | 'Disabled' | 'SendDisabled' | 'ReceiveDisabled';

type Props = {
  name: string;
  namespace: string;
  status: QueueStatus;
};

const statusConfig: Record<
  QueueStatus,
  { rootClass: string; statusClass: string; label: string }
> = {
  Active: { rootClass: styles.rootActive, statusClass: styles.statusActive, label: 'Active' },
  Disabled: {
    rootClass: styles.rootDisabled,
    statusClass: styles.statusDisabled,
    label: 'Disabled',
  },
  SendDisabled: {
    rootClass: styles.rootWarning,
    statusClass: styles.statusWarning,
    label: 'Send Disabled',
  },
  ReceiveDisabled: {
    rootClass: styles.rootWarning,
    statusClass: styles.statusWarning,
    label: 'Receive Disabled',
  },
};

export function ServiceBusQueueNode({ name, namespace, status }: Props) {
  const { rootClass, statusClass, label } = statusConfig[status];
  return (
    <div className={`${styles.root} ${rootClass}`}>
      <div className={styles.header}>
        <img
          src="/icons/azure/10836-icon-service-Azure-Service-Bus.svg"
          alt=""
          className={styles.icon}
        />
        <span className={styles.name}>{name}</span>
      </div>
      <div className={styles.queueInfo}>{namespace} / Service Bus Queue</div>
      <div className={`${styles.status} ${statusClass}`}>
        <span className={styles.statusDot} />
        {label}
      </div>
    </div>
  );
}
