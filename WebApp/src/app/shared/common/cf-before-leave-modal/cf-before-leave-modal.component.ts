import { Component, OnInit,Input,Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
declare var $: any;
@Component({
  selector: 'confirm-before-leave-modal',
  templateUrl: './cf-before-leave-modal.component.html',
  styleUrls: ['./cf-before-leave-modal.component.scss']
})
export class CfBeforeLeaveModalComponent implements OnInit {

  constructor(private router: Router) { }

  IDModalToLeave:string = null;
  MessageToShow:string = "All entered data will be discarded. Are you sure want to leave ?";
  UrlToNavigate:string = null;

  @Input() set ModalToLeaveID(id:string){
    if(id!==null)
      this.IDModalToLeave = id;
  }

  @Input() set Message(mess:string){
    if(mess!==null)
      this.MessageToShow = mess;
  }

  @Input() set NavigateTo(route:string){
    if(route!==null)
      this.UrlToNavigate = route;
  }

  @Output() isLeave = new EventEmitter<boolean>();

  ngOnInit() {
    
  }


  Yes(){
    this.isLeave.emit(true);
    $('#confirm-before-leave').modal('hide');
    if(this.IDModalToLeave!==null){
      $('#'+ this.IDModalToLeave).modal('hide')
    }
    if(this.UrlToNavigate!==null){
      this.router.navigateByUrl(this.UrlToNavigate);
      setTimeout(() => {
      }, 400);
     
    }

  }

  No(){
    this.isLeave.emit(false);
    $('#confirm-before-leave').modal('hide');
  }

}
