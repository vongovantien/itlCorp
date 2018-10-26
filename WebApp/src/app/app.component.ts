import { Component,OnInit } from '@angular/core';
import { API_MENU } from '../constants/api-menu.const';
import { SystemConstants } from '../constants/system.const';


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
    
  }
  constructor(private api_menu:API_MENU){

  }

  title = 'app';
}
