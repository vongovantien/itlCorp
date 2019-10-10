import { NgModule } from "@angular/core";
import { OperationRoutingModule } from "./operation-routing.module";
import { TruckingAssignmentComponent } from "./trucking-assignment/trucking-assignment.component";

@NgModule({
    imports: [
        OperationRoutingModule,
    ],
    declarations: [
        TruckingAssignmentComponent,
    ]
})
export class OperationModule { }
