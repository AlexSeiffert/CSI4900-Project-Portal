const mounted = new WeakMap();

function token(name) {
  return getComputedStyle(document.documentElement).getPropertyValue(`--color-${name}`).trim();
}

function color(value) {
  if (!value || value.startsWith('#') || value.startsWith('rgb')) return value;
  const match = value.match(/^var\(--color-(.+)\)$/);
  return token(match ? match[1] : value);
}

function percent(part, whole) {
  return whole ? `${Math.round((100 * part) / whole)}%` : '0%';
}

function mount(el) {
  let entry = mounted.get(el);
  if (!entry) {
    const chart = echarts.init(el, null, { renderer: 'svg' });
    const observer = new ResizeObserver(() => chart.resize());
    observer.observe(el);
    entry = { chart, observer };
    mounted.set(el, entry);
  }
  return entry.chart;
}

const FONT = '"Work Sans", ui-sans-serif, system-ui, sans-serif';

function base() {
  const reduced = matchMedia('(prefers-reduced-motion: reduce)').matches;
  return {
    textStyle: { fontFamily: FONT, color: token('text') },
    animation: !reduced,
    animationDuration: 800,
    animationEasing: 'quarticOut',
    animationDurationUpdate: 400,
    animationEasingUpdate: 'quarticOut',
    tooltip: {
      backgroundColor: token('dark'),
      borderColor: token('border'),
      borderWidth: 1,
      padding: [8, 12],
      textStyle: { color: token('text'), fontFamily: FONT, fontSize: 12, lineHeight: 18 },
      extraCssText: 'border-radius:10px;box-shadow:0 10px 30px rgb(0 0 0 / 0.4);',
    },
  };
}

function axis(extra = {}) {
  return {
    axisLine: { show: false },
    axisTick: { show: false },
    axisLabel: { color: token('text-secondary'), fontSize: 11, fontFamily: FONT },
    splitLine: { lineStyle: { color: token('border'), opacity: 0.6 } },
    ...extra,
  };
}

function legend(show) {
  return show
    ? { top: 0, left: 0, icon: 'circle', itemWidth: 8, itemHeight: 8, itemGap: 16, textStyle: { color: token('text-secondary'), fontSize: 12, fontFamily: FONT } }
    : { show: false };
}

const dot = (c) => `<span style="display:inline-block;width:8px;height:8px;border-radius:50%;margin-right:6px;background:${c}"></span>`;

export function sankey(el, { nodes, links, labelRoom = 150, leftLabels = false }) {
  const shared = base();
  const valueOf = Object.fromEntries(nodes.map((n) => [n.name, n.value]));
  const colorOf = Object.fromEntries(nodes.map((n) => [n.name, color(n.color)]));

  mount(el).setOption(
    {
      ...shared,
      tooltip: {
        ...shared.tooltip,
        trigger: 'item',
        formatter: (p) =>
          p.dataType === 'edge'
            ? `${p.data.source} → ${p.data.target}<br><b style="font-size:15px">${p.data.value}</b>` +
              `&nbsp;&nbsp;<span style="opacity:.7">${percent(p.data.value, valueOf[p.data.source])} of ${p.data.source}</span>`
            : `${p.name}<br><b style="font-size:15px">${p.value}</b>`,
      },
      series: [
        {
          type: 'sankey',
          animationDuration: shared.animationDuration,
          animationEasing: shared.animationEasing,
          left: leftLabels ? 190 : 0,
          right: labelRoom,
          top: 6,
          bottom: 6,
          nodeWidth: 14,
          nodeGap: 14,
          nodeAlign: 'left',
          layoutIterations: 0,
          draggable: false,
          emphasis: { focus: 'adjacency' },
          blur: { lineStyle: { opacity: 0.08 }, itemStyle: { opacity: 0.35 } },
          label: {
            position: 'right',
            distance: 10,
            formatter: (p) => `{name|${p.name}}  {value|${p.value}}`,
            rich: {
              name: { fontWeight: 600, fontSize: 13, color: token('text') },
              value: { fontSize: 13, color: token('text-secondary') },
            },
          },
          lineStyle: { curveness: 0.5 },
          data: nodes.map((n) => ({
            name: n.name, value: n.value, depth: n.depth, itemStyle: { color: colorOf[n.name], borderWidth: 0 },
            label: leftLabels && n.depth === 0 ? { position: 'left' } : undefined,
          })),
          links: links.map((l) => ({
            source: l.source,
            target: l.target,
            value: l.value,
            lineStyle:
              l.color === 'gradient' ? { color: 'gradient', opacity: 0.5 }
              : l.color === 'source' ? { color: colorOf[l.source], opacity: 0.35 }
              : l.color === 'target' ? { color: colorOf[l.target], opacity: 0.4 }
              : { color: color(l.color), opacity: 0.3 },
          })),
        },
      ],
    },
    true,
  );
}

export function bars(el, { categories, series, horizontal = false, stacked = false, unit = '', max, legend: showLegend, barWidth, links }) {
  const withLegend = showLegend ?? series.length > 1;
  const shared = base();
  const categoryAxis = axis({ type: 'category', data: categories, inverse: horizontal, splitLine: { show: false }, triggerEvent: !!links,
    axisLabel: { ...axis().axisLabel, color: token(horizontal ? 'text' : 'text-secondary'), fontSize: horizontal ? 12 : 11 } });
  const valueAxis = axis({ type: 'value', minInterval: 1, max, axisLabel: { ...axis().axisLabel, formatter: `{value}${unit}` } });
  const last = series.length - 1;
  const chart = mount(el);

  chart.off('click');
  if (links) {
    chart.on('click', (p) => {
      const index = p.componentType === 'series' ? p.dataIndex : categories.indexOf(p.value);
      const href = links[index];
      if (!href) return;
      if (window.Blazor?.navigateTo) window.Blazor.navigateTo(href);
      else location.assign(href);
    });
  }

  chart.setOption(
    {
      ...shared,
      legend: legend(withLegend),
      grid: { left: 0, right: 18, top: withLegend ? 34 : 8, bottom: 0, containLabel: true },
      tooltip: {
        ...shared.tooltip,
        trigger: 'axis',
        axisPointer: { type: 'shadow', shadowStyle: { color: 'rgba(255,255,255,0.06)' } },
        formatter: (items) =>
          `<b>${items[0].axisValue}</b><br>` + items.map((i) => `${dot(i.color)}${i.seriesName} <b>${i.value}${unit}</b>`).join('<br>'),
      },
      xAxis: horizontal ? valueAxis : categoryAxis,
      yAxis: horizontal ? categoryAxis : valueAxis,
      series: series.map((s, index) => s.type === 'line' ? {
        name: s.name,
        type: 'line',
        symbol: 'circle',
        symbolSize: 7,
        z: 3,
        cursor: links ? 'pointer' : 'default',
        lineStyle: { color: color(s.color), width: 2 },
        itemStyle: { color: color(s.color) },
        emphasis: { focus: 'series' },
        data: s.data,
      } : {
        name: s.name,
        type: 'bar',
        stack: stacked ? 'all' : undefined,
        cursor: links ? 'pointer' : 'default',
        barMaxWidth: barWidth ?? (horizontal ? 16 : 28),
        barGap: '20%',
        itemStyle: {
          color: color(s.color),
          borderRadius: !stacked || index === last ? (horizontal ? [0, 4, 4, 0] : [4, 4, 0, 0]) : 0,
        },
        emphasis: { focus: 'series' },
        data: s.data.map((d) => (typeof d === 'object' && d !== null ? { value: d.value, itemStyle: { color: color(d.color) } } : d)),
      }),
    },
    true,
  );
}

export function donut(el, { segments, caption = '' }) {
  const shared = base();
  const total = segments.reduce((sum, s) => sum + s.value, 0);

  mount(el).setOption(
    {
      ...shared,
      tooltip: {
        ...shared.tooltip,
        trigger: 'item',
        formatter: (p) => `${dot(p.color)}${p.name}<br><b style="font-size:15px">${p.value}</b>&nbsp;&nbsp;<span style="opacity:.7">${percent(p.value, total)}</span>`,
      },
      title: {
        text: `${total}`,
        subtext: caption,
        left: 'center',
        top: 'middle',
        itemGap: 2,
        textStyle: { fontFamily: FONT, fontSize: 40, fontWeight: 600, color: token('text') },
        subtextStyle: { fontFamily: FONT, fontSize: 13, color: token('text-secondary') },
      },
      series: [
        {
          type: 'pie',
          radius: ['64%', '92%'],
          padAngle: 2,
          itemStyle: { borderRadius: 4 },
          label: { show: false },
          emphasis: { scale: true, scaleSize: 4 },
          data: segments.filter((s) => s.value > 0).map((s) => ({ name: s.name, value: s.value, itemStyle: { color: color(s.color) } })),
        },
      ],
    },
    true,
  );
}

export function dispose(el) {
  const entry = mounted.get(el);
  if (entry) {
    entry.observer.disconnect();
    entry.chart.dispose();
    mounted.delete(el);
  }
}
