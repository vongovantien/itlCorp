import { Component, OnInit, Input } from '@angular/core';
import * as $ from 'jquery';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit {

  @Input() Page_Info: String;

  english_flag = "assets/app/media/img/lang/en.png";
  vietnam_flag = "assets/app/media/img/lang/vi.png";
  active_flag = "assets/app/media/img/lang/en.png";

  constructor(private route: ActivatedRoute,private router:Router) { }

  ngOnInit() {
    if (localStorage.getItem("CURRENT_CLIENT_LANGUAGE") === "en") {
      this.active_flag = this.english_flag;
    }
    if (localStorage.getItem("CURRENT_CLIENT_LANGUAGE") === "vi") {
      this.active_flag = this.vietnam_flag;
    }
  }

  changeLanguage(lang) {
    if (lang === localStorage.getItem("CURRENT_CLIENT_LANGUAGE")) {
      return
    } else {
      if (lang === "en") {
        localStorage.setItem("CURRENT_CLIENT_LANGUAGE", "en");       
        const url = window.location.protocol + "//" + window.location.hostname + "/" + lang +"/#" + this.router.url + "/";
        console.log(url);
        this.active_flag = this.english_flag;
        window.location.href = url;
      }
      if (lang === "vi") {
        localStorage.setItem("CURRENT_CLIENT_LANGUAGE", "vi");
        const url = window.location.protocol + "//" + window.location.hostname + "/" + lang +"/#" + this.router.url + "/";
        console.log(url);
        this.active_flag = this.vietnam_flag;
        window.location.href = url;
      }
    }
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
