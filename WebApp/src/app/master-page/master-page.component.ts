import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';


@Component({
  selector: 'app-master-page',
  templateUrl: './master-page.component.html',
  styleUrls: ['./master-page.component.css']
})
export class MasterPageComponent implements OnInit {

  constructor(private baseService: BaseService) { }

  async ngOnInit() {
    var url = "https://api.github.com/repositories/19438/issues"
    var url_club = "https://gola-server.herokuapp.com/api/club/create";
    var issues = await this.baseService.getAsync(url, null, true);

    this.baseService.get(url).subscribe(data=>{
      console.log(data);
    })

    
    console.log(issues);
    console.log("hi");

  }


}
