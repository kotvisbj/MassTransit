Under the hood
===========================================

MassTransit hides all the details of messages and delivery from the developer.  However, when there are issues it is important to know how it works so you can troubleshoot the issues.  


Setup
---------------
To see how this plays out, consider the following message types:

.. sourcecode:: csharp

	namespace MT.Messages {
		interface ISomeMessage {}
	}

Configure the bus that will listen to ``ISomeMessage`` with an endpoint of ``my_endpoint``.


Starting a bus 
---------------
When creating the endpoint above you indicated the name of the queue where the messages will end up.  See queue naming rules below.  Starting the bus with the consumers registered, causes the following configuration to happen:

- A queue named ``my_endpoint``is created for all messages on this endpoint
- An exchange named ``my_endpoint`` is created for all messages on this endpoint
- An exchange named ``MT.Messages.ISomeMessage``is created for the message
- An exchange to exchange binding from ``MT.Messages.ISomeMessage`` to ``my_endpoint`` is created
- A binding from the ``my_endpoint`` exchange to ``my_endpoint`` queue is created.

.. note::

    All exchanges created are of type FanOut

Publishing a message
---------------

When you publish a message on the bus here is what happens

- Publish ``MT.Messages.ISomeMessage``
- This message gets published by the publishing logic to the exchange ``MT.Messages.ISomeMessage``.
- The message is routed by messaging infrastructure to the ``my_endpoint`` exchange.
- The message is then routed to the ``my_endpoint`` queue.

.. note::

If you publish a message before the consumer has been started (and created its configuration), the exchange ``MT.Messages.ISomeMessage`` will be created.  It will not be bound to anything until the consumer starts, so if you publish to it, the message will just disappear.


Queues
---------------

- Each application you write should use a unique queue name.
- If you run multiple copies  of your consumer service, they would listen to the same queue (as they are copies).
	This would mean you have multiple applications listening to ``my_endpoint`` queue
	This would result in a 'competing consumer' scenario.  (Which is what you want if you run same service multiple times)
- If there is an exceptioh from your consumer, the message will be sent to ``my_endpoint_error`` queue.
- If a message is received in a queue that the consumer does not know how to handle, the message will be sent to ``my_endpoint_skipped`` queue.


Design Benefits
---------------
- Any application can listen to any message and that will not affect any other application that may or may not be listening for that message
- Any application(s) that bind a group of messages to the same queue will result in the competing consumer pattern.
- You do not have to concern yourself with anything but what message type to produce and what message type to consume.



Faq
----------------------------------------------
- How many messages at a time will be simultaniously processed?
		- Each endpoint you create represents 1 queue.  That queue can receive any number of differnt message types (based on what you subscribe to it)
		- The configuration of each endpoint you can set the number of consumers with a call to ``PrefetchCount(x)``.  
		- This is the total number of consumers for all message types sent to this queue.
		- In MT2, you had to add ?prefetch=X to the Rabbit URL.  This is handled automatically now.
- Can I have a set number of consumers per message type?
		Yes.  This uses middleware.  ``x.Consumer(new AutofacConsumerFactory<…>(), p => p.UseConcurrencyLimit(1));  x.PrefetchCount=16;``
		PrefetchCount should be relatively high, a multiple of your concurrency limit for all message types so that RabbitMQ doesn’t choke delivery messages due to network delays. Always have a queue ready to receive the message.
- When my consumer is not running, I do not want the messages to wait in the queue.  How can I do this?
	There are two ways.  Note that each of these imply you would never use a 'competing consumer' pattern, so make sure that is the case.
		1.  Set ``PurgetOnStartup=true`` in the endpoint configuration.  When the bus starts, it will empty the queue of all messages.
		2.  Set ``AutoDelete=true`` in the endpoint configuration.  This causes the queue to be removed when your application stops.
- How are Retrys handled?
	This is handled by `middleware`_  Each endpoint has a `retry policy`_.  
- Can I have a different retry policy per each message type?  
	No.  This is set at an endpoint level.  You would have to have a specific queue per consumer to achieve this.  

.. _retry policy: ../usage/retry.html
.. _middleware: ../middleware/retry.html


