import { Component, OnInit, Input } from '@angular/core';
import * as $ from 'jquery'; 
import * as lodash from 'lodash';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit {

  @Input() Page_Info : String;

  constructor() { }

  ngOnInit() {
  }

  
  //minimize sidebar
  minimize_page_sidebar() {
    var bodyElement = document.getElementById('bodyElement');
    var leftMinimizeToggle = document.getElementById('m_aside_left_minimize_toggle');
    if (leftMinimizeToggle.classList.contains('m-brand__toggler--active')) {
      bodyElement.classList.remove('m-brand--minimize', 'm-aside-left--minimize');
      leftMinimizeToggle.classList.remove('m-brand__toggler--active');
    } else {
      bodyElement.classList.add('m-brand--minimize', 'm-aside-left--minimize');
      leftMinimizeToggle.classList.add('m-brand__toggler--active');
    }
  }
}
