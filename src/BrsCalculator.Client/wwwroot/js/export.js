window.brsExport = {
  loadScript: function (src) {
    return new Promise((resolve, reject) => {
      const s = document.createElement('script');
      s.src = src;
      s.onload = resolve;
      s.onerror = reject;
      document.head.appendChild(s);
    });
  },

  ensureLibs: async function () {
    if (typeof html2canvas === 'undefined') {
      await this.loadScript('https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js');
    }
    if (typeof window.jspdf === 'undefined') {
      await this.loadScript('https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js');
    }
  },

  exportElement: async function (elementId, format, fileName) {
    const element = document.getElementById(elementId);
    if (!element) return;

    await this.ensureLibs();

    const canvas = await html2canvas(element, { scale: 2, backgroundColor: '#ffffff' });
    const imgData = canvas.toDataURL('image/png');

    if (format === 'png') {
      const link = document.createElement('a');
      link.download = `${fileName}.png`;
      link.href = imgData;
      link.click();
      return;
    }

    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF('p', 'mm', 'a4');
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();
    const imgWidth = pageWidth - 20;
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    pdf.addImage(imgData, 'PNG', 10, 10, imgWidth, Math.min(imgHeight, pageHeight - 20));
    pdf.save(`${fileName}.pdf`);
  }
};
