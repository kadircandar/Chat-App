import { Injectable } from '@angular/core';
import Swal, { SweetAlertIcon } from 'sweetalert2';
@Injectable({
  providedIn: 'root',
})
export class ToastService {
  constructor() {}

  showToast(message: string, icon: SweetAlertIcon = 'success') {
    const Toast = Swal.mixin({
      toast: true,
      position: 'top-end',
      showConfirmButton: false,
      timer: 3000,
      timerProgressBar: true,
      didOpen: (toast) => {
        toast.onmouseenter = Swal.stopTimer;
        toast.onmouseleave = Swal.resumeTimer;
      },
    });

    Toast.fire({
      icon: icon,
      title: message,
    });
  }

  // Kısa yol metodlar (opsiyonel)
  showSuccess(message: string) {
    this.showToast(message, 'success');
  }

  showError(message: string) {
    this.showToast(message, 'error');
  }

  showInfo(message: string) {
    this.showToast(message, 'info');
  }

  showWarning(message: string) {
    this.showToast(message, 'warning');
  }
}
