// Bridge between Blazor and ECharts (loaded by index.html). Blazor sends the data (Components/EChart.razor
// calls the function named by Kind); the look lives here. Colours are read from the --color-* tokens in
// Styles/app.css, so charts always match the site and will follow it to a dark theme.

const mounted = new WeakMap();

function token(name) {
  return getComputedStyle(document.documentElement).getPropertyValue(`--color-${name}`).trim();
}

// Accepts a token name ("mono-3"), a CSS variable ("var(--color-stage-2)") or a plain colour.
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

// Settings every chart shares: Work Sans, a dark rounded tooltip and a quick ease-out entrance.
function base() {
  const reduced = matchMedia('(prefers-reduced-motion: reduce)').matches;
  return {
    textStyle: { fontFamily: FONT, color: token('dark') },
    animation: !reduced,
    animationDuration: 800,
    animationEasing: 'quarticOut',
    animationDurationUpdate: 400,
    animationEasingUpdate: 'quarticOut',
    tooltip: {
      backgroundColor: token('dark'),
      borderWidth: 0,
      padding: [8, 12],
      textStyle: { color: token('white'), fontFamily: FONT, fontSize: 12, lineHeight: 18 },
      extraCssText: 'border-radius:10px;box-shadow:0 10px 30px rgb(45 45 44 / 0.22);',
    },
  };
}

function axis(extra = {}) {
  return {
    axisLine: { show: false },
    axisTick: { show: false },
    axisLabel: { color: token('muted'), fontSize: 11, fontFamily: FONT },
    splitLine: { lineStyle: { color: token('divider'), opacity: 0.6 } },
    ...extra,
  };
}

function legend(show) {
  return show
    ? { top: 0, left: 0, icon: 'circle', itemWidth: 8, itemHeight: 8, itemGap: 16, textStyle: { color: token('muted'), fontSize: 12, fontFamily: FONT } }
    : { show: false };
}

const dot = (c) => `<span style="display:inline-block;width:8px;height:8px;border-radius:50%;margin-right:6px;background:${c}"></span>`;

// Flow chart. nodes: [{ name, value, depth, color }]; links: [{ source, target, value, color }] where a
// link's color is "gradient" (blend its ends), "source" or "target" (that end's colour) or a colour/token.
// leftLabels puts the first column's labels on its left, so they don't sit on the ribbons.
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
          // Sankey series default to their own slower, linear entrance; match the bars.
          animationDuration: shared.animationDuration,
          animationEasing: shared.animationEasing,
          left: leftLabels ? 190 : 0, // room for "Software Engineering 58"
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
              name: { fontWeight: 600, fontSize: 13, color: token('dark') },
              value: { fontSize: 13, color: token('muted') },
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

// Bar chart. series: [{ name, color, data, type }] where each value is a number or { value, color };
// type 'line' draws that series as a line with dots over the bars.
// horizontal puts categories down the side (first at the top); stacked piles the series up;
// unit is added after values ("%"); max caps the value axis (100 for percentages). legend defaults
// to showing only with two or more series; barWidth overrides the default bar thickness. links holds a
// page for each category: clicking its label or bars opens it.
export function bars(el, { categories, series, horizontal = false, stacked = false, unit = '', max, legend: showLegend, barWidth, links }) {
  const withLegend = showLegend ?? series.length > 1;
  const shared = base();
  const categoryAxis = axis({ type: 'category', data: categories, inverse: horizontal, splitLine: { show: false }, triggerEvent: !!links,
    axisLabel: { ...axis().axisLabel, color: token(horizontal ? 'dark' : 'muted'), fontSize: horizontal ? 12 : 11 } });
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
      grid: { left: 0, right: 18, top: withLegend ? 34 : 8, bottom: 0, containLabel: true }, // right: room for "100%"
      tooltip: {
        ...shared.tooltip,
        trigger: 'axis',
        axisPointer: { type: 'shadow', shadowStyle: { color: 'rgba(45,45,44,0.05)' } },
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
          // Round only the outer end of a stack
          borderRadius: !stacked || index === last ? (horizontal ? [0, 4, 4, 0] : [4, 4, 0, 0]) : 0,
        },
        emphasis: { focus: 'series' },
        data: s.data.map((d) => (typeof d === 'object' && d !== null ? { value: d.value, itemStyle: { color: color(d.color) } } : d)),
      }),
    },
    true,
  );
}

// Ring of the shares of a whole, with the total in the middle. segments: [{ name, value, color }];
// caption sits under the total ("projects"). The legend is left to the page, so it can sit beside or below.
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
        textStyle: { fontFamily: FONT, fontSize: 40, fontWeight: 600, color: token('dark') },
        subtextStyle: { fontFamily: FONT, fontSize: 13, color: token('muted') },
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
