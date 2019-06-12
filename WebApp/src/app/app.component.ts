
import { Component, OnInit} from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { Router} from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  ngOnInit(): void {   
    document.body.style.zoom = "100%"
    if(!this.baseService.checkLoginSession()){
      this.router.navigateByUrl('/login');
    }
  }

  public BreadcrumbStack:{path:string,name:string,level:number}[]=[];

  constructor(private baseService:BaseService, private router:Router) {
  }


}
