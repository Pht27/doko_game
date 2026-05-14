import { useState, useMemo } from 'react';
import './SortableTable.css';

export interface Column<T> {
  key: string;
  label: string;
  headerLabel?: string;
  sortValue?: (row: T) => number | string | null | undefined;
  render?: (row: T) => React.ReactNode;
  align?: 'left' | 'right';
  width?: string;
}

interface Props<T> {
  columns: Column<T>[];
  rows: T[];
  initialSort?: string;
  defaultDir?: 'asc' | 'desc';
  rowKey?: (row: T, i: number) => string | number;
}

export function SortableTable<T>({
  columns,
  rows,
  initialSort,
  defaultDir = 'desc',
  rowKey,
}: Props<T>) {
  const [sort, setSort] = useState<{ key: string; dir: 'asc' | 'desc' } | null>(
    initialSort ? { key: initialSort, dir: defaultDir } : null,
  );

  const sorted = useMemo(() => {
    if (!sort) return rows;
    const col = columns.find((c) => c.key === sort.key);
    if (!col) return rows;
    const getValue =
      col.sortValue ?? ((r: T) => (r as Record<string, unknown>)[sort.key] as number | string | null);
    const dir = sort.dir === 'asc' ? 1 : -1;
    return [...rows].sort((a, b) => {
      const va = getValue(a), vb = getValue(b);
      if (va == null && vb == null) return 0;
      if (va == null) return 1;
      if (vb == null) return -1;
      if (typeof va === 'number' && typeof vb === 'number') return (va - vb) * dir;
      return String(va).localeCompare(String(vb)) * dir;
    });
  }, [rows, sort, columns]);

  const onHeaderClick = (key: string) => {
    setSort((cur) => {
      if (!cur || cur.key !== key) return { key, dir: defaultDir };
      if (cur.dir === defaultDir) return { key, dir: defaultDir === 'desc' ? 'asc' : 'desc' };
      return null;
    });
  };

  return (
    <table className="st-table">
      <thead>
        <tr>
          {columns.map((c, i) => {
            const active = sort?.key === c.key;
            const align = c.align ?? (i === 0 ? 'left' : 'right');
            return (
              <th
                key={c.key}
                onClick={() => onHeaderClick(c.key)}
                className={`st-th${active ? ' st-th-active' : ''}`}
                style={{ textAlign: align, width: c.width }}
              >
                <span className="st-th-inner">
                  {c.headerLabel ?? c.label}
                  <span className="st-sort-icon">
                    {active ? (sort?.dir === 'desc' ? '▼' : '▲') : '▾'}
                  </span>
                </span>
              </th>
            );
          })}
        </tr>
      </thead>
      <tbody>
        {sorted.map((row, ri) => (
          <tr key={rowKey ? rowKey(row, ri) : ri} className="st-tr">
            {columns.map((c, i) => {
              const align = c.align ?? (i === 0 ? 'left' : 'right');
              const isFirst = i === 0;
              return (
                <td
                  key={c.key}
                  className={`st-td${isFirst ? ' st-td-label' : ' st-td-num'}${ri === sorted.length - 1 ? ' st-td-last' : ''}`}
                  style={{ textAlign: align }}
                >
                  {c.render ? c.render(row) : String((row as Record<string, unknown>)[c.key] ?? '')}
                </td>
              );
            })}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
