import { Component,OnInit } from '@angular/core';
import { API_MENU } from '../constants/api-menu.const';
import { SystemConstants } from '../constants/system.const';
import { Router, RouteConfigLoadStart, RouteConfigLoadEnd } from '@angular/router';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  ngOnInit(): void {
 
    if(localStorage.getItem(SystemConstants.CURRENT_LANGUAGE)==null){
      localStorage.setItem("CURRENT_LANGUAGE", SystemConstants.DEFAULT_LANGUAGE);
    }
    if(localStorage.getItem(SystemConstants.CURRENT_VERSION)==null){
      localStorage.setItem("CURRENT_VERSION","1");
    }    

    this.router.events.subscribe(event => {
      if (event instanceof RouteConfigLoadStart) {
          this.spinnerService.show();
      } else if (event instanceof RouteConfigLoadEnd) {
          this.spinnerService.hide();
      }
  });
    
  }
  constructor(private api_menu:API_MENU,private router: Router,private spinnerService: Ng4LoadingSpinnerService){

  }

  title = 'app';
}
