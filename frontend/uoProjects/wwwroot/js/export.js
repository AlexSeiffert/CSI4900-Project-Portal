window.tableExport = (() => {
  const SHEETJS = 'https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js';
  const JSPDF = 'https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js';
  const AUTOTABLE = 'https://cdnjs.cloudflare.com/ajax/libs/jspdf-autotable/3.8.2/jspdf.plugin.autotable.min.js';
  const scripts = {};

  function load(src) {
    scripts[src] ??= new Promise((resolve, reject) => {
      const el = document.createElement('script');
      el.src = src;
      el.onload = resolve;
      el.onerror = () => { delete scripts[src]; reject(new Error(`Could not load ${src}`)); };
      document.head.append(el);
    });
    return scripts[src];
  }

  const safe = (v) => (typeof v === 'string' && /^[=+\-@\t\r]/.test(v) ? `'${v}` : v);

  function save(blob, filename) {
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    document.body.append(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(a.href), 1000);
  }

  const csvCell = (v) => {
    const text = String(safe(v) ?? '');
    return /[",\r\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
  };

  async function download(format, name, headers, rows) {
    const stamp = new Date().toISOString().slice(0, 10);
    const file = `${name}-${stamp}`;
    const title = name.replace(/-/g, ' ').replace(/^./, (c) => c.toUpperCase());

    if (format === 'json') {
      const data = rows.map((r) => Object.fromEntries(headers.map((h, i) => [h, r[i]])));
      save(new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' }), `${file}.json`);
    } else if (format === 'excel') {
      await load(SHEETJS);
      const sheet = window.XLSX.utils.aoa_to_sheet([headers, ...rows.map((r) => r.map(safe))]);
      const book = window.XLSX.utils.book_new();
      window.XLSX.utils.book_append_sheet(book, sheet, title.slice(0, 31));
      window.XLSX.writeFile(book, `${file}.xlsx`);
    } else if (format === 'pdf') {
      await load(JSPDF);
      await load(AUTOTABLE);
      const doc = new window.jspdf.jsPDF({ orientation: headers.length > 5 ? 'landscape' : 'portrait', unit: 'pt', format: 'a4' });
      doc.setFontSize(14);
      doc.text(`${title} · ${stamp}`, 40, 42);
      doc.autoTable({
        head: [headers],
        body: rows.map((r) => r.map((v) => (v == null ? '' : String(v)))),
        startY: 58,
        styles: { fontSize: 9, cellPadding: 5 },
        headStyles: { fillColor: [143, 0, 26] },
        margin: { left: 40, right: 40 },
      });
      doc.save(`${file}.pdf`);
    } else {
      const lines = [headers, ...rows].map((r) => r.map(csvCell).join(','));
      save(new Blob(['\ufeff' + lines.join('\r\n')], { type: 'text/csv;charset=utf-8' }), `${file}.csv`);
    }
  }

  return { download };
})();
