import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'chatapp-ui';

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit() {
    const token = this.authService.getToken() || ''
    if (!token || this.authService.isTokenExpired(token)) {
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }
}
