import { Injectable } from '@angular/core';
import { CanActivate,Router} from '@angular/router';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuardService implements CanActivate {

  canActivate(): boolean{
    if(!this.baseService.checkLoginSession(false)){
      this.router.navigate(['/login']);
      return false;
    }else{
      return true;
    }
  }

  constructor(private baseService:BaseService,private router:Router) { }

}





