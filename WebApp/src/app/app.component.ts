import { Component, OnInit } from '@angular/core';
import { SystemConstants } from '../constants/system.const';
import { BaseService } from 'src/services-base/base.service';
import { Router } from '@angular/router';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  ngOnInit(): void {
    /**
     * Check login status 
     */
    if(this.baseService.checkLoginSession(false)){
      this.router.navigateByUrl('/home');
    };
  }

  constructor(
    private router:Router,
    private baseService:BaseService) {
  }


}
