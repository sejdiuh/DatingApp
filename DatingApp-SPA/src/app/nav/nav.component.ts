import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;

  constructor(public authService: AuthService, private alertify: AlertifyService, private route: Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  logIn() {
    this.authService.logIn(this.model).subscribe(next => {
      this.alertify.success('Loggedin successfully');
    }, error => {
      this.alertify.error(error);
    }, () => {
      this.route.navigate(['/members']);
    });
  }
  loggedIn() {
    return this.authService.loggedIn();
  }

  logOut() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authService.decodetToken = null;
    this.authService.currentUser = null;
    this.alertify.message('Logged out');
    this.route.navigate(['/home']);
  }
}
