import { Component, OnInit } from '@angular/core';
import * as Validate from 'src/helper/ValidateHelper';
import * as SearchHelper from 'src/helper/SearchHelper';

@Component({
  selector: 'app-master-page',
  templateUrl: './master-page.component.html',
  styleUrls: ['./master-page.component.css']
})
export class MasterPageComponent implements OnInit {
  email= "";
  phone= "";
  users = [
    {email:'abc@gmail.com',phone:'0978780912'},
    {email:'dangthe@gamil.com',phone:'01662236296'},
    {email:'ngochien@gmail.com',phone:'01635985253'},
    {email:'ronaldo@yahoo.com',phone:'098372512'},
    {email:'messi@outlook.com',phone:'06284920902'}
  ];
  const_users = [
    {email:'abc@gmail.com',phone:'0978780912'},
    {email:'dangthe@gamil.com',phone:'01662236296'},
    {email:'ngochien@gmail.com',phone:'01635985253'},
    {email:'ronaldo@yahoo.com',phone:'098372512'},
    {email:'messi@outlook.com',phone:'06284920902'}
  ];

  constructor() { }

  ngOnInit() {
  }

  submit(){
    console.log(Validate.ValidateEmail(this.email));
    console.log(Validate.ValidatePhoneNumber(this.phone.toString()));
  }

  list_keys_search:any[]=[];
  async Search(event,fields,return_list,condition){
    
    var value = event.target.value.trim();
    if(return_list=='user'){
      this.list_keys_search = await SearchHelper.PrepareListFieldSearch(this.list_keys_search,fields,value,condition);
      var reference_source = this.const_users.map(x=>Object.assign({},x));
      this.users = await SearchHelper.SearchEngine(this.list_keys_search,reference_source,condition);
    }

  }
}
