/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.ServiceModel;
using www.tibco.com.BookOrderService;
using com.tibco.wcf.tems;

namespace BookOrderService
{
    [ServiceContract(Name = "IBookOrder", Namespace = "http://www.tibco.com/BookOrderService")]
    public interface IBookOrder
    {
        [OperationContract(Action = "\"/BookOrderService/BookOrderService.serviceagent/BookOrderPTEndpoint2/orderBook\"")]
        [FaultContract(typeof(www.tibco.com.BookOrderService.orderBook_fault), Action = "orderBook", Name = "orderBook_fault")]
        orderBookResponse orderBook(orderBookRequest request);

        [OperationContract(Action = "\"/BookOrderService/BookOrderService.serviceagent/BookOrderPTEndpoint2/searchBook\"")]
        searchBookResponse searchBook(searchBookRequest request);
    }

    [ServiceBehavior(Namespace = "http://www.tibco.com/BookOrderService")]
    public class BookServiceType : IBookOrder
    {
        private BookServiceImpl service;
        
        #region Constructors
        public BookServiceType()
        {
            service = new BookServiceImpl();
        }

        #endregion

        #region BookOrderPT Members

        public orderBookResponse orderBook(orderBookRequest request)
        {
            orderBookResponse reply;
            reply = service.orderBook(request);

#if ManualAcknowledgeSample
            //Manual Acknowledge Sample
            SendAcknowledge();
#endif
            return reply;
        }

        #endregion

        #region BookSearchPT Members

        public searchBookResponse searchBook(searchBookRequest request)
        {
            searchBookResponse reply = service.searchBook(request);

#if ManualAcknowledgeSample
            //Manual Acknowledge Sample
            SendAcknowledge();
#endif
            return reply;
        }

        #endregion

        private void SendAcknowledge()
        {
            object msgProperty;
            TemsMessage temsMessage = null;

            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(TemsMessage.key, out msgProperty))
            {
                temsMessage = (TemsMessage)msgProperty;
                temsMessage.Acknowledge();
            }

        }

        public string TestThis(string request)
        {
            return "Static Test String";
        }
    }

    class BookServiceImpl
    {
        #region Constructors
        public BookServiceImpl()
        {
        }

        #endregion

        #region BookOrderPT Members

        public orderBookResponse orderBook(orderBookRequest request)
        {
            orderBookResponse resp = new orderBookResponse();
            resp.Book = new Book();
            resp.Book.authors = new ArrayOfString();
            resp.Book.authors.Add("Test.Author1");
            resp.Book.authors.Add("Test.Author2");
            resp.Book.isbn = "some bogus isbn: 87wehru37482j";
            resp.Book.price = 50.99;
            resp.Book.publisher = "Book publisher here.";
            resp.Book.title = "Sunshine over Falling Rock Bluff";
            resp.discount = 0;
            resp.orderNumber = "12345";
            resp.orderTotal = 1000.00;
            resp.quantity = 324;
            resp.ShippingDetails = new ShippingDetails();
            resp.ShippingDetails.shippingAddress = "123 N. American Rd.";
            resp.ShippingDetails.shippingDate = "Right Away";
            resp.ShippingDetails.shipTo = "Fred Flintstone";
            resp.unitPrice = 123.4;
            return resp;
        }

        #endregion

        #region BookSearchPT Members

        public searchBookResponse searchBook(searchBookRequest request)
        {
            Book book1 = new Book();
            Book book2 = new Book();
            Books books = new Books();
            books.Add(book1);
            books.Add(book2);
            return new searchBookResponse(books);
        }

        #endregion
    }
}