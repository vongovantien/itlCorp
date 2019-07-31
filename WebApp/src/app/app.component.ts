
import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  ngOnInit(): void {
    document.body.style.zoom = "100%"
    if (!this.baseService.checkLoginSession()) {
      this.router.navigateByUrl('/login');
    }
  }


  constructor(private baseService: BaseService, private router: Router) {
  }


}
