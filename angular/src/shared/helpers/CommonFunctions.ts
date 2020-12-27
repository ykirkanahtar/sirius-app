import { Injectable } from '@angular/core';
import * as moment from 'moment';

@Injectable()
export class CommonFunctions {

    static toMoment(date: Date): moment.Moment {
        if (date === null) {
          throw new Error("date cannot be null");
        }
    
        var month = date.getMonth() + 1; //months from 1-12
        var day = date.getDate();
        var year = date.getFullYear();
    
        return moment({ year: year, month: month - 1, day: day }).add(1, 'days');
      }
}

