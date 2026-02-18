// Simple accordion: only one open at a time
document.addEventListener('DOMContentLoaded', function(){
  const items = document.querySelectorAll('.accordion-item');
  items.forEach(item => {
    const btn = item.querySelector('.accordion-toggle');
    const panel = item.querySelector('.accordion-panel');
    btn.addEventListener('click', ()=>{
      const expanded = btn.getAttribute('aria-expanded') === 'true';
      // close all
      items.forEach(i=>{
        const b = i.querySelector('.accordion-toggle');
        const p = i.querySelector('.accordion-panel');
        b.setAttribute('aria-expanded','false');
        p.style.display = 'none';
      });
      if(!expanded){
        btn.setAttribute('aria-expanded','true');
        panel.style.display = 'block';
      }
    });
    // init closed
    panel.style.display = 'none';
  });
});
