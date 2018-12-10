import { Component, OnInit, Input, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'lodash';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit,AfterViewInit {
  ngAfterViewInit(): void {
        
    if (this.getCurrentLangFromUrl() === "en") {
      this.active_flag = this.english_flag;
      localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE,"en");
    }
    if (this.getCurrentLangFromUrl() === "vi") {
      this.active_flag = this.vietnam_flag;
      localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE,"vi");
    }
  }

  @Input() Page_Info: String;

  english_flag = "assets/app/media/img/lang/en.png";
  vietnam_flag = "assets/app/media/img/lang/vi.png";
  active_flag = "assets/app/media/img/lang/en.png";

  constructor(private router:Router) { }

  ngOnInit() {

  }

  getCurrentLangFromUrl(){
    const url = window.location.href;
    const host = window.location.hostname; 
    const url_arr = url.split("/");
    var current_lang_index =  url_arr.indexOf(host)+1; 

    console.log({URL:url,HOST:host,URL_ARR:url_arr,CURRENT_CLIENT_LANGUAGE:url_arr[current_lang_index],INDEX:current_lang_index});
    return url_arr[current_lang_index];
  }

  changeLanguage(lang) {
    if (lang === localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE)) {
      return
    } else {
      if (lang === "en") {
        localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "en");       
        const url = window.location.protocol + "//" + window.location.hostname + "/" + lang +"/#" + this.router.url + "/";
        this.active_flag = this.english_flag;
        window.location.href = url;
      }
      if (lang === "vi") {
        localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "vi");
        const url = window.location.protocol + "//" + window.location.hostname + "/" + lang +"/#" + this.router.url + "/";
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
