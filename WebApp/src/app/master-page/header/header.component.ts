import { Component, OnInit,Input } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  // styleUrls: ['./header.component.css','assets/style-share/header/_header.scss']
  // styleUrls:['../../../assets/style-share/header/_header.scss']
})
export class HeaderComponent implements OnInit {

  @Input() Page_Info : String;

  constructor() { }

  ngOnInit() {
  }

}
