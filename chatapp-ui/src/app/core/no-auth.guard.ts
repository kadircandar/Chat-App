import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class NoAuthGuard implements CanActivate {
  constructor(private router: Router, private authService: AuthService) {}

  canActivate(): boolean {

    if (this.authService.getToken()) {
      // Token varsa login veya register’a gitmesine izin verme
      this.router.navigate(['/dashboard']);
      return false;
    }
    return true;
  }
}
