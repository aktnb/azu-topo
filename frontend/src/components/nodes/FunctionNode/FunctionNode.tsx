import styles from "./FunctionNode.module.scss";

type Props = {
  name: string;
  functionAppName: string;
  enabled: boolean;
};

export function FunctionNode({ name, functionAppName, enabled }: Props) {
  return (
    <div className={`${styles.root} ${enabled ? styles.rootEnabled : styles.rootDisabled}`}>
      <div className={styles.header}>
        <img
          src="/icons/azure/10029-icon-service-Function-Apps.svg"
          alt=""
          className={styles.icon}
        />
        <span className={styles.name}>{name}</span>
      </div>
      <div className={styles.functionApp}>{functionAppName} / Function</div>
      <div className={`${styles.status} ${enabled ? styles.statusEnabled : styles.statusDisabled}`}>
        <span className={styles.statusDot} />
        {enabled ? "Enabled" : "Disabled"}
      </div>
    </div>
  );
}
