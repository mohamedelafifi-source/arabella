// Arabella — password dialog and smooth scroll
(function () {
  var PASSWORD = 'arabella';

  var dialog = document.getElementById('password-dialog');
  var form = document.getElementById('password-form');
  var input = document.getElementById('password-input');
  var errorEl = document.getElementById('password-error');
  var unlocked = false;

  function showError(show) {
    errorEl.hidden = !show;
    if (show) input.setAttribute('aria-invalid', 'true');
    else input.removeAttribute('aria-invalid');
  }

  function grantAccess() {
    unlocked = true;
    dialog.close();
  }

  dialog.addEventListener('close', function () {
    if (!unlocked) dialog.showModal();
    showError(false);
    input.value = '';
  });

  form.addEventListener('submit', function (e) {
    e.preventDefault();
    showError(false);
    if (input.value === PASSWORD) {
      grantAccess();
    } else {
      showError(true);
      input.focus();
    }
  });

  function openDialog() {
    dialog.showModal();
    input.focus();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', openDialog);
  } else {
    openDialog();
  }

  document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
    anchor.addEventListener('click', function (e) {
      var targetId = this.getAttribute('href');
      if (targetId === '#') return;
      var target = document.querySelector(targetId);
      if (target) {
        e.preventDefault();
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });
})();
