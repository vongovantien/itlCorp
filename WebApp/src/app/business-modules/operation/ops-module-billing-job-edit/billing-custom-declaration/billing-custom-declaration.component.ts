import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';

@Component({
  selector: 'app-billing-custom-declaration',
  templateUrl: './billing-custom-declaration.component.html',
  styleUrls: ['./billing-custom-declaration.component.scss']
})
export class BillingCustomDeclarationComponent implements OnInit {
  currentJob: OpsTransaction;
  constructor(private baseServices: BaseService) { }

  ngOnInit() {
    this.stateChecking();
  }

  stateChecking() {
    setTimeout(() => {
      this.baseServices.dataStorage.subscribe(data => {
        if(data["CurrentOpsTransaction"] != null){
          console.log('custom clearance');
          this.currentJob = data["CurrentOpsTransaction"];
          console.log(this.currentJob);
        }
      }); 
    }, 1000);
  }
}
