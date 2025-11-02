import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { ToastService } from 'src/app/services/toast.service';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css'],
})
export class AuthComponent {
  username = '';
  password = '';
  isRegisterMode = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {}

  toggleMode(): void {
    this.isRegisterMode = !this.isRegisterMode;
  }

  submit(): void {
    if (this.isRegisterMode) {
      this.authService.register(this.username, this.password).subscribe({
        next: () => {

          this.toastService.showSuccess('Kayıt başarılı! Lütfen giriş yapın');
          this.isRegisterMode = false;
        },
        error: (err) => {
          this.toastService.showError(err.error);
        },
      });
    } else {
      this.authService.login(this.username, this.password).subscribe({
        next: (res: any) => {
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.toastService.showError(err.error);
        },
      });
    }
  }
}
