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
    if(!this.baseService.checkLoginSession(false)){
      this.router.navigateByUrl('/login');
    }
  }

  constructor(private baseService:BaseService, private router:Router) {
  }


}
